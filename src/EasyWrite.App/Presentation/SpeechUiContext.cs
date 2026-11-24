using EasyWrite.Core.Models;

namespace EasyWrite.App.Presentation;

/// <summary>Inputs used to derive every enabled state in the speech workspace.</summary>
public sealed record SpeechUiContext(
    SpeechOperationState OperationState,
    bool HasText,
    bool HasTranscript,
    bool CanSynthesize,
    bool CanRecognize);
