namespace EasyWrite.App.Presentation;

/// <summary>Immutable control state rendered by the Windows Forms views.</summary>
public sealed record SpeechControlState(
    bool TextEditorEnabled,
    bool SpeechSettingsEnabled,
    bool VoiceSelectionEnabled,
    bool SpeakEnabled,
    bool PauseResumeEnabled,
    string PauseResumeText,
    bool StopSpeechEnabled,
    bool ClearTextEnabled,
    bool RecognizerSelectionEnabled,
    bool StartListeningEnabled,
    bool StopListeningEnabled,
    bool ClearTranscriptEnabled,
    bool ExportEnabled,
    string StatusText,
    string AccessibleStatus);
