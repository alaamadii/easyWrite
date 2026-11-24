using EasyWrite.Core.Models;

namespace EasyWrite.Core.Events;

public sealed class SpeechResultEventArgs(SpeechResult result) : EventArgs
{
    public SpeechResult Result { get; } = result;
}
