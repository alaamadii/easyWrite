namespace EasyWrite.Core.Models;

/// <summary>A finalized speech-recognition result.</summary>
/// <param name="Text">Recognized text.</param>
/// <param name="Timestamp">Local time at which recognition completed.</param>
/// <param name="Confidence">Engine confidence from 0.0 through 1.0.</param>
public sealed record SpeechResult(string Text, DateTimeOffset Timestamp, float Confidence);
