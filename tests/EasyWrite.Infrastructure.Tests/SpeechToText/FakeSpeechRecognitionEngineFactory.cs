using System.Globalization;
using EasyWrite.Infrastructure.SpeechToText;

namespace EasyWrite.Infrastructure.Tests.SpeechToText;

internal sealed class FakeSpeechRecognitionEngineFactory : ISpeechRecognitionEngineFactory
{
    public List<RecognizerDescriptor> Recognizers { get; } =
    [
        new RecognizerDescriptor(
            "test-en-US",
            "Test English Recognizer",
            CultureInfo.GetCultureInfo("en-US"),
            "Deterministic English recognizer")
    ];

    public List<FakeSpeechRecognitionEngineAdapter> CreatedEngines { get; } = [];

    public string? CreatedRecognizerId { get; private set; }

    public Exception? EnumerationException { get; set; }

    public Action<FakeSpeechRecognitionEngineAdapter>? ConfigureEngine { get; set; }

    public IReadOnlyList<RecognizerDescriptor> GetInstalledRecognizers()
    {
        if (EnumerationException is not null)
        {
            throw EnumerationException;
        }

        return Recognizers;
    }

    public ISpeechRecognitionEngineAdapter Create(string recognizerId)
    {
        CreatedRecognizerId = recognizerId;
        var engine = new FakeSpeechRecognitionEngineAdapter();
        ConfigureEngine?.Invoke(engine);
        CreatedEngines.Add(engine);
        return engine;
    }
}

internal sealed class FakeSpeechRecognitionEngineAdapter : ISpeechRecognitionEngineAdapter
{
    private EventHandler<RecognitionResultEventArgs>? _speechRecognized;
    private EventHandler<RecognitionCompletedEventArgs>? _recognitionCompleted;

    public event EventHandler<RecognitionResultEventArgs>? SpeechRecognized
    {
        add
        {
            _speechRecognized += value;
            ResultSubscriptionCount++;
        }

        remove
        {
            _speechRecognized -= value;
            ResultUnsubscriptionCount++;
        }
    }

    public event EventHandler<RecognitionCompletedEventArgs>? RecognitionCompleted
    {
        add
        {
            _recognitionCompleted += value;
            CompletionSubscriptionCount++;
        }

        remove
        {
            _recognitionCompleted -= value;
            CompletionUnsubscriptionCount++;
        }
    }

    public int ResultSubscriptionCount { get; private set; }

    public int ResultUnsubscriptionCount { get; private set; }

    public int CompletionSubscriptionCount { get; private set; }

    public int CompletionUnsubscriptionCount { get; private set; }

    public bool DictationGrammarLoaded { get; private set; }

    public bool DefaultInputSelected { get; private set; }

    public int StartCount { get; private set; }

    public int CancelCount { get; private set; }

    public int DisposeCount { get; private set; }

    public Exception? GrammarException { get; set; }

    public Exception? InputException { get; set; }

    public Exception? StartException { get; set; }

    public Exception? CancelException { get; set; }

    public void LoadDictationGrammar()
    {
        if (GrammarException is not null)
        {
            throw GrammarException;
        }

        DictationGrammarLoaded = true;
    }

    public void SetInputToDefaultAudioDevice()
    {
        if (InputException is not null)
        {
            throw InputException;
        }

        DefaultInputSelected = true;
    }

    public void StartContinuousRecognition()
    {
        if (StartException is not null)
        {
            throw StartException;
        }

        StartCount++;
    }

    public void CancelRecognition()
    {
        if (CancelException is not null)
        {
            throw CancelException;
        }

        CancelCount++;
    }

    public void Dispose() => DisposeCount++;

    public void EmitResult(string text, float confidence = 0.8F) =>
        _speechRecognized?.Invoke(this, new RecognitionResultEventArgs(text, confidence));

    public void Complete(bool cancelled = true, Exception? error = null) =>
        _recognitionCompleted?.Invoke(this, new RecognitionCompletedEventArgs(cancelled, error));
}
