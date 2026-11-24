using System.Speech.Recognition;

namespace EasyWrite.Infrastructure.SpeechToText;

internal sealed class SystemSpeechRecognitionEngineAdapter : ISpeechRecognitionEngineAdapter
{
    private readonly SpeechRecognitionEngine _engine;
    private bool _disposed;

    public SystemSpeechRecognitionEngineAdapter(string recognizerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recognizerId);
        _engine = new SpeechRecognitionEngine(recognizerId);
        _engine.SpeechRecognized += HandleSpeechRecognized;
        _engine.RecognizeCompleted += HandleRecognizeCompleted;
    }

    public event EventHandler<RecognitionResultEventArgs>? SpeechRecognized;

    public event EventHandler<RecognitionCompletedEventArgs>? RecognitionCompleted;

    public void LoadDictationGrammar()
    {
        ThrowIfDisposed();
        _engine.LoadGrammar(new DictationGrammar());
    }

    public void SetInputToDefaultAudioDevice()
    {
        ThrowIfDisposed();
        _engine.SetInputToDefaultAudioDevice();
    }

    public void StartContinuousRecognition()
    {
        ThrowIfDisposed();
        _engine.RecognizeAsync(RecognizeMode.Multiple);
    }

    public void CancelRecognition()
    {
        ThrowIfDisposed();
        _engine.RecognizeAsyncCancel();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _engine.SpeechRecognized -= HandleSpeechRecognized;
        _engine.RecognizeCompleted -= HandleRecognizeCompleted;
        try
        {
            _engine.UnloadAllGrammars();
        }
        finally
        {
            _engine.Dispose();
        }
    }

    private void HandleSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        SpeechRecognized?.Invoke(
            this,
            new RecognitionResultEventArgs(e.Result.Text, e.Result.Confidence));
    }

    private void HandleRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        RecognitionCompleted?.Invoke(
            this,
            new RecognitionCompletedEventArgs(e.Cancelled, e.Error));
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
