namespace EasyWrite.Core.Models;

/// <summary>Represents the single active speech operation shown to the user.</summary>
public enum SpeechOperationState
{
    Idle,
    Speaking,
    Paused,
    Listening,
    Stopping,
    Error
}
