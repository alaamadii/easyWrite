using EasyWrite.Core.Models;

namespace EasyWrite.App.Presentation;

/// <summary>
/// Converts one authoritative operation state into consistent enabled states for
/// both speech modules.
/// </summary>
public static class SpeechControlStateMapper
{
    public static SpeechControlState Map(SpeechUiContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var isIdle = context.OperationState == SpeechOperationState.Idle;
        var isError = context.OperationState == SpeechOperationState.Error;
        var canAcceptNewOperation = isIdle || isError;
        var isSpeaking = context.OperationState == SpeechOperationState.Speaking;
        var isPaused = context.OperationState == SpeechOperationState.Paused;
        var isListening = context.OperationState == SpeechOperationState.Listening;
        var isStopping = context.OperationState == SpeechOperationState.Stopping;
        var isTextLocked = isSpeaking || isPaused || isStopping;

        var (statusText, accessibleStatus) = GetStatus(context.OperationState);

        return new SpeechControlState(
            TextEditorEnabled: !isTextLocked,
            SpeechSettingsEnabled: !isTextLocked,
            VoiceSelectionEnabled: !isTextLocked && context.CanSynthesize,
            SpeakEnabled: canAcceptNewOperation && context.CanSynthesize && context.HasText,
            PauseResumeEnabled: isSpeaking || isPaused,
            PauseResumeText: isPaused ? "&Resume" : "&Pause",
            StopSpeechEnabled: isSpeaking || isPaused,
            ClearTextEnabled: !isTextLocked && context.HasText,
            RecognizerSelectionEnabled: !isListening && !isStopping && context.CanRecognize,
            StartListeningEnabled: canAcceptNewOperation && context.CanRecognize,
            StopListeningEnabled: isListening,
            ClearTranscriptEnabled: !isListening && !isStopping && context.HasTranscript,
            ExportEnabled: false,
            StatusText: statusText,
            AccessibleStatus: accessibleStatus);
    }

    private static (string StatusText, string AccessibleStatus) GetStatus(
        SpeechOperationState operationState) => operationState switch
        {
            SpeechOperationState.Idle => ("Ready", "EasyWrite is ready. No speech operation is active."),
            SpeechOperationState.Speaking => ("Speaking", "EasyWrite is speaking the entered text."),
            SpeechOperationState.Paused => ("Paused", "Text-to-speech playback is paused."),
            SpeechOperationState.Listening => ("Listening", "The microphone is active and EasyWrite is listening."),
            SpeechOperationState.Stopping => ("Stopping", "EasyWrite is safely stopping the active operation."),
            SpeechOperationState.Error => ("Attention needed", "The last operation needs attention before retrying."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operationState),
                operationState,
                "Unsupported speech operation state.")
        };
}
