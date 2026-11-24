namespace EasyWrite.Infrastructure.SpeechToText;

internal sealed class RecognitionCompletedEventArgs(bool cancelled, Exception? error) : EventArgs
{
    public bool Cancelled { get; } = cancelled;

    public Exception? Error { get; } = error;
}
