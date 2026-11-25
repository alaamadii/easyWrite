namespace EasyWrite.Infrastructure.SpeechToText;

internal sealed class RecognitionResultEventArgs(string text, float confidence) : EventArgs
{
    public string Text { get; } = text;

    public float Confidence { get; } = confidence;
}
