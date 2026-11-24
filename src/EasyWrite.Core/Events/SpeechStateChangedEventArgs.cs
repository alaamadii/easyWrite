using EasyWrite.Core.Models;

namespace EasyWrite.Core.Events;

public sealed class SpeechStateChangedEventArgs(
    SpeechOperationState previousState,
    SpeechOperationState currentState,
    string statusMessage) : EventArgs
{
    public SpeechOperationState PreviousState { get; } = previousState;

    public SpeechOperationState CurrentState { get; } = currentState;

    public string StatusMessage { get; } = statusMessage;
}
