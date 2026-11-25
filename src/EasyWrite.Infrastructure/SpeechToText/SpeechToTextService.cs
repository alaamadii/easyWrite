using System.Globalization;
using EasyWrite.Core.Events;
using EasyWrite.Core.Models;
using EasyWrite.Core.Services;

namespace EasyWrite.Infrastructure.SpeechToText;

public sealed class SpeechToTextService : ISpeechToTextService
{
    private readonly Lock _syncRoot = new();
    private readonly ISpeechRecognitionEngineFactory _factory;
    private ISpeechRecognitionEngineAdapter? _activeEngine;
    private TaskCompletionSource? _stopCompletionSource;
    private CancellationTokenRegistration _cancellationRegistration;
    private SpeechOperationState _state = SpeechOperationState.Idle;
    private bool _disposed;

    public SpeechToTextService()
        : this(new SystemSpeechRecognitionEngineFactory())
    {
    }

    internal SpeechToTextService(ISpeechRecognitionEngineFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public event EventHandler<SpeechResultEventArgs>? ResultRecognized;

    public event EventHandler<SpeechStateChangedEventArgs>? StateChanged;

    public IReadOnlyList<CultureInfo> GetInstalledRecognizerCultures()
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();

            try
            {
                return GetRecognizers()
                    .Select(recognizer => recognizer.Culture)
                    .DistinctBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(culture => culture.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                    .ToArray();
            }
            catch (Exception exception) when (exception is not ObjectDisposedException)
            {
                throw new InvalidOperationException(
                    "Windows speech recognizers could not be loaded. Install a speech language and restart EasyWrite.",
                    exception);
            }
        }
    }

    public Task StartListeningAsync(
        CultureInfo culture,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(culture);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            if (_activeEngine is not null ||
                _state is SpeechOperationState.Listening or SpeechOperationState.Stopping)
            {
                throw new InvalidOperationException(
                    "Microphone recognition is already active. Stop the current session before starting again.");
            }

            RecognizerDescriptor recognizer;
            try
            {
                recognizer = ResolveRecognizer(GetRecognizers(), culture);
            }
            catch (InvalidOperationException exception)
            {
                var validationEvent = TransitionTo(SpeechOperationState.Error, exception.Message);
                RaiseStateChanged(validationEvent);
                throw;
            }
            catch (Exception exception)
            {
                const string message =
                    "Windows speech recognizers could not be loaded. Install a speech language and restart EasyWrite.";
                var discoveryEvent = TransitionTo(SpeechOperationState.Error, message);
                RaiseStateChanged(discoveryEvent);
                throw new InvalidOperationException(message, exception);
            }

            ISpeechRecognitionEngineAdapter? engine = null;
            try
            {
                engine = _factory.Create(recognizer.Id);
                engine.SpeechRecognized += HandleSpeechRecognized;
                engine.RecognitionCompleted += HandleRecognitionCompleted;
                engine.LoadDictationGrammar();
                engine.SetInputToDefaultAudioDevice();
                _activeEngine = engine;
                var listeningEvent = TransitionTo(
                    SpeechOperationState.Listening,
                    "Listening through the default microphone.");
                engine.StartContinuousRecognition();

                if (ReferenceEquals(_activeEngine, engine))
                {
                    RaiseStateChanged(listeningEvent);
                }
            }
            catch (Exception exception)
            {
                if (engine is not null)
                {
                    engine.SpeechRecognized -= HandleSpeechRecognized;
                    engine.RecognitionCompleted -= HandleRecognitionCompleted;
                    engine.Dispose();
                }

                _activeEngine = null;
                var stateEvent = TransitionTo(
                    SpeechOperationState.Error,
                    "The microphone could not be started. Check Windows microphone access and your default input device.");
                RaiseStateChanged(stateEvent);
                throw new InvalidOperationException(
                    "The microphone could not be started. Check Windows microphone access, the default input device, and the selected speech language.",
                    exception);
            }

            if (_activeEngine is not null && cancellationToken.CanBeCanceled)
            {
                _cancellationRegistration = cancellationToken.Register(RequestCancellation);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopListeningAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            if (_activeEngine is null)
            {
                return Task.CompletedTask;
            }

            if (_state == SpeechOperationState.Stopping)
            {
                var existingStopTask = _stopCompletionSource?.Task ?? Task.CompletedTask;
                return cancellationToken.CanBeCanceled
                    ? existingStopTask.WaitAsync(cancellationToken)
                    : existingStopTask;
            }

            if (_state != SpeechOperationState.Listening)
            {
                return Task.CompletedTask;
            }

            _stopCompletionSource = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var stoppingEvent = TransitionTo(
                SpeechOperationState.Stopping,
                "Stopping microphone recognition.");
            RaiseStateChanged(stoppingEvent);

            try
            {
                _activeEngine.CancelRecognition();
            }
            catch (Exception exception)
            {
                return FailActiveSession(
                    "Microphone recognition could not be stopped cleanly.",
                    exception);
            }

            return cancellationToken.CanBeCanceled
                ? _stopCompletionSource.Task.WaitAsync(cancellationToken)
                : _stopCompletionSource.Task;
        }
    }

    public void Dispose()
    {
        ISpeechRecognitionEngineAdapter? engine;
        TaskCompletionSource? stopCompletionSource;

        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            engine = _activeEngine;
            _activeEngine = null;
            stopCompletionSource = _stopCompletionSource;
            _stopCompletionSource = null;
            _cancellationRegistration.Dispose();
            _cancellationRegistration = default;

            if (engine is not null)
            {
                engine.SpeechRecognized -= HandleSpeechRecognized;
                engine.RecognitionCompleted -= HandleRecognitionCompleted;
                try
                {
                    engine.CancelRecognition();
                }
                catch (Exception)
                {
                    // Disposal continues when Windows has already released the input device.
                }

                engine.Dispose();
            }
        }

        stopCompletionSource?.TrySetCanceled();
    }

    private RecognizerDescriptor[] GetRecognizers() =>
        _factory.GetInstalledRecognizers()
            .Where(recognizer =>
                !string.IsNullOrWhiteSpace(recognizer.Id) &&
                !string.IsNullOrWhiteSpace(recognizer.Culture.Name))
            .OrderBy(
                recognizer => recognizer.Culture.DisplayName,
                StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(recognizer => recognizer.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

    private static RecognizerDescriptor ResolveRecognizer(
        RecognizerDescriptor[] recognizers,
        CultureInfo requestedCulture)
    {
        if (recognizers.Length == 0)
        {
            throw new InvalidOperationException(
                "No compatible Windows speech recognizer was found. Install a speech language in Windows Settings and restart EasyWrite.");
        }

        return recognizers.FirstOrDefault(recognizer =>
                   string.Equals(
                       recognizer.Culture.Name,
                       requestedCulture.Name,
                       StringComparison.OrdinalIgnoreCase))
               ?? throw new InvalidOperationException(
                   "The selected recognition language is no longer available. Choose another installed language.");
    }

    private void HandleSpeechRecognized(object? sender, RecognitionResultEventArgs e)
    {
        SpeechResultEventArgs? resultEvent = null;

        lock (_syncRoot)
        {
            if (_disposed ||
                _state != SpeechOperationState.Listening ||
                !ReferenceEquals(sender, _activeEngine) ||
                string.IsNullOrWhiteSpace(e.Text))
            {
                return;
            }

            resultEvent = new SpeechResultEventArgs(new SpeechResult(
                e.Text.Trim(),
                DateTimeOffset.Now,
                Math.Clamp(e.Confidence, 0F, 1F)));
        }

        ResultRecognized?.Invoke(this, resultEvent);
    }

    private void HandleRecognitionCompleted(object? sender, RecognitionCompletedEventArgs e)
    {
        ISpeechRecognitionEngineAdapter? engine;
        TaskCompletionSource? stopCompletionSource;
        SpeechStateChangedEventArgs stateEvent;

        lock (_syncRoot)
        {
            if (_disposed || !ReferenceEquals(sender, _activeEngine))
            {
                return;
            }

            engine = _activeEngine;
            _activeEngine = null;
            stopCompletionSource = _stopCompletionSource;
            _stopCompletionSource = null;
            _cancellationRegistration.Dispose();
            _cancellationRegistration = default;
            engine!.SpeechRecognized -= HandleSpeechRecognized;
            engine.RecognitionCompleted -= HandleRecognitionCompleted;

            stateEvent = e.Error is null
                ? TransitionTo(
                    SpeechOperationState.Idle,
                    "Microphone recognition stopped.")
                : TransitionTo(
                    SpeechOperationState.Error,
                    "Recognition failed. Check your microphone and installed speech language.");
        }

        engine.Dispose();

        if (e.Error is null)
        {
            stopCompletionSource?.TrySetResult();
        }
        else
        {
            stopCompletionSource?.TrySetException(new InvalidOperationException(
                "Recognition failed. Check the microphone and installed speech language, then try again.",
                e.Error));
        }

        RaiseStateChanged(stateEvent);
    }

    private Task FailActiveSession(string message, Exception exception)
    {
        var engine = _activeEngine;
        _activeEngine = null;
        var stopCompletionSource = _stopCompletionSource;
        _stopCompletionSource = null;
        _cancellationRegistration.Dispose();
        _cancellationRegistration = default;

        if (engine is not null)
        {
            engine.SpeechRecognized -= HandleSpeechRecognized;
            engine.RecognitionCompleted -= HandleRecognitionCompleted;
            engine.Dispose();
        }

        var failure = new InvalidOperationException(message, exception);
        stopCompletionSource?.TrySetException(failure);
        var stateEvent = TransitionTo(SpeechOperationState.Error, message);
        RaiseStateChanged(stateEvent);
        return stopCompletionSource?.Task ?? Task.FromException(failure);
    }

    private void RequestCancellation()
    {
        try
        {
            _ = ObserveCancellationStopAsync(StopListeningAsync());
        }
        catch (ObjectDisposedException)
        {
            // Disposal won the race with cancellation.
        }
    }

    private static async Task ObserveCancellationStopAsync(Task stopTask)
    {
        try
        {
            await stopTask;
        }
        catch (Exception exception) when (exception is OperationCanceledException or InvalidOperationException)
        {
            // StateChanged already reports stop failures; cancellation has no caller to await them.
        }
    }

    private SpeechStateChangedEventArgs TransitionTo(
        SpeechOperationState nextState,
        string statusMessage)
    {
        var previousState = _state;
        _state = nextState;
        return new SpeechStateChangedEventArgs(previousState, nextState, statusMessage);
    }

    private void RaiseStateChanged(SpeechStateChangedEventArgs eventArgs)
    {
        if (eventArgs.PreviousState != eventArgs.CurrentState)
        {
            StateChanged?.Invoke(this, eventArgs);
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
