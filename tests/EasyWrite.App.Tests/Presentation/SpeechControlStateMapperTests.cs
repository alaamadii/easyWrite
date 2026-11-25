using EasyWrite.App.Presentation;
using EasyWrite.Core.Models;
using Xunit;

namespace EasyWrite.App.Tests.Presentation;

public sealed class SpeechControlStateMapperTests
{
    [Fact]
    public void IdleWithUnavailableServicesKeepsOperationsDisabled()
    {
        var result = Map(SpeechOperationState.Idle, hasText: true, hasTranscript: true);

        Assert.True(result.TextEditorEnabled);
        Assert.True(result.SpeechSettingsEnabled);
        Assert.True(result.ClearTextEnabled);
        Assert.True(result.ClearTranscriptEnabled);
        Assert.False(result.VoiceSelectionEnabled);
        Assert.False(result.RecognizerSelectionEnabled);
        Assert.False(result.SpeakEnabled);
        Assert.False(result.StartListeningEnabled);
        Assert.False(result.ExportEnabled);
        Assert.Equal("Ready", result.StatusText);
    }

    [Fact]
    public void IdleWithCapabilitiesEnablesValidStartActions()
    {
        var context = new SpeechUiContext(
            SpeechOperationState.Idle,
            HasText: true,
            HasTranscript: false,
            CanSynthesize: true,
            CanRecognize: true);

        var result = SpeechControlStateMapper.Map(context);

        Assert.True(result.VoiceSelectionEnabled);
        Assert.True(result.RecognizerSelectionEnabled);
        Assert.True(result.SpeakEnabled);
        Assert.True(result.StartListeningEnabled);
        Assert.False(result.PauseResumeEnabled);
        Assert.False(result.StopSpeechEnabled);
        Assert.False(result.StopListeningEnabled);
    }

    [Fact]
    public void SpeakingLocksTextAndEnablesPlaybackControls()
    {
        var result = Map(SpeechOperationState.Speaking, hasText: true);

        Assert.False(result.TextEditorEnabled);
        Assert.False(result.SpeechSettingsEnabled);
        Assert.True(result.PauseResumeEnabled);
        Assert.Equal("&Pause", result.PauseResumeText);
        Assert.True(result.StopSpeechEnabled);
        Assert.False(result.StartListeningEnabled);
        Assert.Equal("Speaking", result.StatusText);
    }

    [Fact]
    public void PausedUsesResumeAction()
    {
        var result = Map(SpeechOperationState.Paused, hasText: true);

        Assert.True(result.PauseResumeEnabled);
        Assert.Equal("&Resume", result.PauseResumeText);
        Assert.True(result.StopSpeechEnabled);
        Assert.Equal("Paused", result.StatusText);
    }

    [Fact]
    public void ListeningEnablesOnlyListeningStopOperation()
    {
        var result = Map(SpeechOperationState.Listening, hasText: true);

        Assert.True(result.TextEditorEnabled);
        Assert.False(result.SpeakEnabled);
        Assert.False(result.PauseResumeEnabled);
        Assert.False(result.StopSpeechEnabled);
        Assert.False(result.StartListeningEnabled);
        Assert.True(result.StopListeningEnabled);
        Assert.False(result.RecognizerSelectionEnabled);
        Assert.Equal("Listening", result.StatusText);
    }

    [Fact]
    public void StoppingLocksInteractiveOperations()
    {
        var result = Map(
            SpeechOperationState.Stopping,
            hasText: true,
            hasTranscript: true,
            canSynthesize: true,
            canRecognize: true);

        Assert.False(result.TextEditorEnabled);
        Assert.False(result.SpeechSettingsEnabled);
        Assert.False(result.SpeakEnabled);
        Assert.False(result.StartListeningEnabled);
        Assert.False(result.StopListeningEnabled);
        Assert.False(result.ClearTextEnabled);
        Assert.False(result.ClearTranscriptEnabled);
        Assert.Equal("Stopping", result.StatusText);
    }

    [Fact]
    public void ErrorAllowsAvailableServicesToRecover()
    {
        var result = Map(
            SpeechOperationState.Error,
            hasText: true,
            canSynthesize: true,
            canRecognize: true);

        Assert.True(result.SpeakEnabled);
        Assert.True(result.StartListeningEnabled);
        Assert.Equal("Attention needed", result.StatusText);
    }

    [Theory]
    [InlineData(SpeechOperationState.Idle, "Ready")]
    [InlineData(SpeechOperationState.Speaking, "Speaking")]
    [InlineData(SpeechOperationState.Paused, "Paused")]
    [InlineData(SpeechOperationState.Listening, "Listening")]
    [InlineData(SpeechOperationState.Stopping, "Stopping")]
    [InlineData(SpeechOperationState.Error, "Attention needed")]
    public void EveryOperationStateHasVisibleAndAccessibleStatus(
        SpeechOperationState operationState,
        string expectedStatus)
    {
        var result = Map(operationState);

        Assert.Equal(expectedStatus, result.StatusText);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessibleStatus));
    }

    private static SpeechControlState Map(
        SpeechOperationState operationState,
        bool hasText = false,
        bool hasTranscript = false,
        bool canSynthesize = false,
        bool canRecognize = false) => SpeechControlStateMapper.Map(
            new SpeechUiContext(
                operationState,
                hasText,
                hasTranscript,
                canSynthesize,
                canRecognize));
}
