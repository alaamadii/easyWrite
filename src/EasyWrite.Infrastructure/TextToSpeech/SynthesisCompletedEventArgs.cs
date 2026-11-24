namespace EasyWrite.Infrastructure.TextToSpeech;

internal sealed class SynthesisCompletedEventArgs(
    object operationToken,
    bool cancelled,
    Exception? error) : EventArgs
{
    public object OperationToken { get; } = operationToken;

    public bool Cancelled { get; } = cancelled;

    public Exception? Error { get; } = error;
}
