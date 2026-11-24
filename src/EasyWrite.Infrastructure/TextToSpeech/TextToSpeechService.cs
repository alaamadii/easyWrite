using EasyWrite.Core.Events;
using EasyWrite.Core.Models;
using EasyWrite.Core.Services;

namespace EasyWrite.Infrastructure.TextToSpeech;

public sealed class TextToSpeechService : ITextToSpeechService
{
    private readonly Lock _syncRoot = new();
    private readonly ISpeechSynthesizerAdapter _synthesizer;
    private CancellationTokenRegistration _cancellationRegistration;
    private TaskCompletionSource? _completionSource;
    private object? _activeOperationToken;
    private SpeechOperationState _state = SpeechOperationState.Idle;
    private bool _disposed;

    public TextToSpeechService()
        : this(new SystemSpeechSynthesizerAdapter())
    {
    }

    internal TextToSpeechService(ISpeechSynthesizerAdapter synthesizer)
    {
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));
        _synthesizer.SpeakCompleted += HandleSpeakCompleted;
    }

    public event EventHandler<SpeechStateChangedEventArgs>? StateChanged;

    public IReadOnlyList<SpeechVoice> GetInstalledVoices()
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();

            try
            {
                return _synthesizer.GetInstalledVoices()
                    .Where(voice => voice.IsEnabled)
                    .OrderBy(voice => voice.Culture, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(voice => voice.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToArray();
            }
            catch (Exception exception) when (exception is not ObjectDisposedException)
            {
                throw new InvalidOperationException(
                    "Windows speech voices could not be loaded. Restart EasyWrite after checking Windows speech settings.",
                    exception);
            }
        }
    }

    public Task SpeakAsync(
        string text,
        SpeechSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text to speak cannot be blank.", nameof(text));
        }

        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();
        cancellationToken.ThrowIfCancellationRequested();

        SpeechStateChangedEventArgs speakingEvent;
        Task completionTask;

        lock (_syncRoot)
        {
            ThrowIfDisposed();

            if (_activeOperationToken is not null ||
                _state is SpeechOperationState.Speaking or
                    SpeechOperationState.Paused or
                    SpeechOperationState.Stopping)
            {
                throw new InvalidOperationException(
                    "Speech is already active. Stop the current playback before starting again.");
            }

            var voices = GetInstalledVoices();
            var voice = ResolveVoice(voices, settings.VoiceName);
            var completionSource = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                _synthesizer.SelectVoice(voice.Name);
                _synthesizer.SetRate(settings.Rate);
                _synthesizer.SetVolume(settings.Volume);
                _synthesizer.SetOutputToDefaultAudioDevice();
                _activeOperationToken = _synthesizer.SpeakAsync(text);
            }
            catch (Exception exception)
            {
                var failure = new InvalidOperationException(
                    "Audio playback could not start. Check the selected voice and default Windows output device, then try again.",
                    exception);
                speakingEvent = TransitionTo(
                    SpeechOperationState.Error,
                    "Speech could not start. Check your voice and audio output.");
                completionSource.SetException(failure);
                RaiseStateChanged(speakingEvent);
                return completionSource.Task;
            }

            _completionSource = completionSource;
            completionTask = completionSource.Task;
            speakingEvent = TransitionTo(SpeechOperationState.Speaking, "Speaking entered text.");
            RaiseStateChanged(speakingEvent);

            if (cancellationToken.CanBeCanceled)
            {
                _cancellationRegistration = cancellationToken.Register(StopSpeech);
            }
        }

        return completionTask;
    }

    public void PauseSpeech()
    {
        SpeechStateChangedEventArgs? stateEvent = null;

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            if (_state != SpeechOperationState.Speaking || _activeOperationToken is null)
            {
                return;
            }

            try
            {
                _synthesizer.Pause();
                stateEvent = TransitionTo(SpeechOperationState.Paused, "Speech playback is paused.");
                RaiseStateChanged(stateEvent);
            }
            catch (Exception exception)
            {
                stateEvent = FailActiveOperation(
                    "Speech could not be paused. Stop playback and try again.",
                    exception);
                RaiseStateChanged(stateEvent);
            }
        }

    }

    public void ResumeSpeech()
    {
        SpeechStateChangedEventArgs? stateEvent = null;

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            if (_state != SpeechOperationState.Paused || _activeOperationToken is null)
            {
                return;
            }

            try
            {
                _synthesizer.Resume();
                stateEvent = TransitionTo(SpeechOperationState.Speaking, "Speech playback resumed.");
                RaiseStateChanged(stateEvent);
            }
            catch (Exception exception)
            {
                stateEvent = FailActiveOperation(
                    "Speech could not be resumed. Stop playback and try again.",
                    exception);
                RaiseStateChanged(stateEvent);
            }
        }

    }

    public void StopSpeech()
    {
        SpeechStateChangedEventArgs? stateEvent = null;

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            if (_activeOperationToken is null || _state == SpeechOperationState.Stopping)
            {
                return;
            }

            if (_state is not SpeechOperationState.Speaking and not SpeechOperationState.Paused)
            {
                return;
            }

            stateEvent = TransitionTo(SpeechOperationState.Stopping, "Stopping speech playback.");
            RaiseStateChanged(stateEvent);
            try
            {
                _synthesizer.CancelAll();
            }
            catch (Exception exception)
            {
                stateEvent = FailActiveOperation(
                    "Speech playback could not be stopped cleanly.",
                    exception);
                RaiseStateChanged(stateEvent);
            }
        }

    }

    public void Dispose()
    {
        TaskCompletionSource? completionSource;

        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            completionSource = _completionSource;
            _completionSource = null;
            _activeOperationToken = null;
            _cancellationRegistration.Dispose();
            _synthesizer.SpeakCompleted -= HandleSpeakCompleted;

            try
            {
                _synthesizer.CancelAll();
            }
            catch (Exception)
            {
                // Disposal must continue even when Windows audio is already unavailable.
            }

            _synthesizer.Dispose();
        }

        completionSource?.TrySetCanceled();
    }

    private static SpeechVoice ResolveVoice(
        IReadOnlyList<SpeechVoice> voices,
        string? requestedVoiceName)
    {
        var enabledVoices = voices.Where(voice => voice.IsEnabled).ToArray();
        if (enabledVoices.Length == 0)
        {
            throw new InvalidOperationException(
                "No enabled Windows voices were found. Install a speech voice in Windows Settings and restart EasyWrite.");
        }

        if (string.IsNullOrWhiteSpace(requestedVoiceName))
        {
            return enabledVoices[0];
        }

        return enabledVoices.FirstOrDefault(
                   voice => string.Equals(
                       voice.Name,
                       requestedVoiceName,
                       StringComparison.OrdinalIgnoreCase))
               ?? throw new InvalidOperationException(
                   "The selected voice is no longer available. Choose another installed voice.");
    }

    private void HandleSpeakCompleted(object? sender, SynthesisCompletedEventArgs e)
    {
        TaskCompletionSource? completionSource;
        SpeechStateChangedEventArgs? stateEvent;
        Exception? error;
        bool cancelled;

        lock (_syncRoot)
        {
            if (_disposed ||
                _activeOperationToken is null ||
                !ReferenceEquals(_activeOperationToken, e.OperationToken))
            {
                return;
            }

            completionSource = _completionSource;
            _completionSource = null;
            _activeOperationToken = null;
            _cancellationRegistration.Dispose();
            _cancellationRegistration = default;
            error = e.Error;
            cancelled = e.Cancelled || _state == SpeechOperationState.Stopping;

            stateEvent = error is null
                ? TransitionTo(
                    SpeechOperationState.Idle,
                    cancelled ? "Speech playback stopped." : "Speech playback completed.")
                : TransitionTo(
                    SpeechOperationState.Error,
                    "Speech playback failed. Check your voice and audio output.");
        }

        if (error is not null)
        {
            completionSource?.TrySetException(new InvalidOperationException(
                "Speech playback failed. Check the selected voice and default Windows output device.",
                error));
        }
        else if (cancelled)
        {
            completionSource?.TrySetCanceled();
        }
        else
        {
            completionSource?.TrySetResult();
        }

        RaiseStateChanged(stateEvent);
    }

    private SpeechStateChangedEventArgs FailActiveOperation(string message, Exception exception)
    {
        try
        {
            _synthesizer.CancelAll();
        }
        catch (Exception)
        {
            // Preserve the original operation failure while still attempting cleanup.
        }

        _completionSource?.TrySetException(new InvalidOperationException(message, exception));
        _completionSource = null;
        _activeOperationToken = null;
        _cancellationRegistration.Dispose();
        _cancellationRegistration = default;
        return TransitionTo(SpeechOperationState.Error, message);
    }

    private SpeechStateChangedEventArgs TransitionTo(
        SpeechOperationState nextState,
        string statusMessage)
    {
        var previousState = _state;
        _state = nextState;
        return new SpeechStateChangedEventArgs(previousState, nextState, statusMessage);
    }

    private void RaiseStateChanged(SpeechStateChangedEventArgs? eventArgs)
    {
        if (eventArgs is not null && eventArgs.PreviousState != eventArgs.CurrentState)
        {
            StateChanged?.Invoke(this, eventArgs);
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
