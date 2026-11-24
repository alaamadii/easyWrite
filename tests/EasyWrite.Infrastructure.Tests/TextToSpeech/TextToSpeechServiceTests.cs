using EasyWrite.Core.Models;
using EasyWrite.Infrastructure.TextToSpeech;
using Xunit;

namespace EasyWrite.Infrastructure.Tests.TextToSpeech;

public sealed class TextToSpeechServiceTests
{
    private static readonly SpeechSettings DefaultSettings = new()
    {
        VoiceName = "Test Voice",
        Rate = 0,
        Volume = 100
    };

    [Fact]
    public void GetInstalledVoicesReturnsEnabledMappedVoicesInStableOrder()
    {
        var adapter = new FakeSpeechSynthesizerAdapter();
        adapter.Voices.Clear();
        adapter.Voices.Add(new SpeechVoice("Zulu", "English", "Zulu description", true));
        adapter.Voices.Add(new SpeechVoice("Disabled", "Arabic", "Disabled description", false));
        adapter.Voices.Add(new SpeechVoice("Alpha", "English", "Alpha description", true));
        using var service = new TextToSpeechService(adapter);

        var voices = service.GetInstalledVoices();

        Assert.Collection(
            voices,
            voice => Assert.Equal(
                new SpeechVoice("Alpha", "English", "Alpha description", true),
                voice),
            voice => Assert.Equal(
                new SpeechVoice("Zulu", "English", "Zulu description", true),
                voice));
    }

    [Fact]
    public void GetInstalledVoicesWhenNoneExistReturnsEmptyCollection()
    {
        var adapter = new FakeSpeechSynthesizerAdapter();
        adapter.Voices.Clear();
        using var service = new TextToSpeechService(adapter);

        Assert.Empty(service.GetInstalledVoices());
    }

    [Fact]
    public void SpeakAsyncWithBlankTextIsRejected()
    {
        using var service = CreateService(out _);

        Assert.Throws<ArgumentException>(() =>
        {
            _ = service.SpeakAsync("  ", DefaultSettings);
        });
    }

    [Fact]
    public void SpeakAsyncValidatesSettingsBeforeUsingAdapter()
    {
        using var service = CreateService(out var adapter);
        var settings = DefaultSettings with { Rate = 11 };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = service.SpeakAsync("Hello", settings);
        });
        Assert.Null(adapter.SpokenText);
    }

    [Fact]
    public void SpeakAsyncForwardsVoiceRateVolumeTextAndOutput()
    {
        using var service = CreateService(out var adapter);
        var settings = DefaultSettings with { Rate = -4, Volume = 35 };

        _ = service.SpeakAsync("Hello EasyWrite", settings);

        Assert.Equal("Test Voice", adapter.SelectedVoice);
        Assert.Equal(-4, adapter.Rate);
        Assert.Equal(35, adapter.Volume);
        Assert.Equal("Hello EasyWrite", adapter.SpokenText);
        Assert.True(adapter.DefaultOutputSelected);
    }

    [Fact]
    public void SpeakAsyncPublishesSpeakingAndCompletionReturnsToIdle()
    {
        using var service = CreateService(out var adapter);
        var states = ObserveStates(service);

        var task = service.SpeakAsync("Hello", DefaultSettings);
        adapter.Complete();

        Assert.True(task.IsCompletedSuccessfully);
        Assert.Equal(
            [SpeechOperationState.Speaking, SpeechOperationState.Idle],
            states);
    }

    [Fact]
    public void PauseAndResumePublishExpectedStates()
    {
        using var service = CreateService(out var adapter);
        var states = ObserveStates(service);
        _ = service.SpeakAsync("Hello", DefaultSettings);

        service.PauseSpeech();
        service.ResumeSpeech();

        Assert.Equal(1, adapter.PauseCount);
        Assert.Equal(1, adapter.ResumeCount);
        Assert.Equal(
            [
                SpeechOperationState.Speaking,
                SpeechOperationState.Paused,
                SpeechOperationState.Speaking
            ],
            states);
    }

    [Fact]
    public async Task StopSpeechPublishesStoppingThenIdleAndCancelsTask()
    {
        using var service = CreateService(out var adapter);
        var states = ObserveStates(service);
        var task = service.SpeakAsync("Hello", DefaultSettings);

        service.StopSpeech();
        service.StopSpeech();
        adapter.Complete(cancelled: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.Equal(1, adapter.CancelCount);
        Assert.Equal(
            [
                SpeechOperationState.Speaking,
                SpeechOperationState.Stopping,
                SpeechOperationState.Idle
            ],
            states);
    }

    [Fact]
    public void SpeakAsyncWhileActiveIsRejected()
    {
        using var service = CreateService(out _);
        _ = service.SpeakAsync("First", DefaultSettings);

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = service.SpeakAsync("Second", DefaultSettings);
        });

        Assert.Contains("already active", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancellationStopsSpeechAndReturnsToIdle()
    {
        using var service = CreateService(out var adapter);
        using var cancellationSource = new CancellationTokenSource();
        var states = ObserveStates(service);
        var task = service.SpeakAsync("Hello", DefaultSettings, cancellationSource.Token);

        cancellationSource.Cancel();
        adapter.Complete(cancelled: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.Equal(1, adapter.CancelCount);
        Assert.Equal(SpeechOperationState.Idle, states[^1]);
    }

    [Fact]
    public async Task AlreadyCancelledRequestDoesNotStartSpeech()
    {
        using var service = CreateService(out var adapter);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.SpeakAsync("Hello", DefaultSettings, cancellationSource.Token));
        Assert.Null(adapter.SpokenText);
    }

    [Fact]
    public async Task SynthesizerErrorPublishesErrorAndFaultsTaskWithInnerException()
    {
        using var service = CreateService(out var adapter);
        var states = ObserveStates(service);
        var task = service.SpeakAsync("Hello", DefaultSettings);
        var sourceError = new InvalidOperationException("Audio device missing");

        adapter.Complete(error: sourceError);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Same(sourceError, exception.InnerException);
        Assert.Equal(SpeechOperationState.Error, states[^1]);
    }

    [Fact]
    public void SpeakAsyncWithUnavailableVoiceIsRejected()
    {
        using var service = CreateService(out _);
        var settings = DefaultSettings with { VoiceName = "Missing Voice" };

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = service.SpeakAsync("Hello", settings);
        });

        Assert.Contains("no longer available", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DisposeCancelsActiveSpeechUnsubscribesAndDisposesAdapter()
    {
        var service = CreateService(out var adapter);
        var task = service.SpeakAsync("Hello", DefaultSettings);

        service.Dispose();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.Equal(1, adapter.UnsubscriptionCount);
        Assert.Equal(1, adapter.CancelCount);
        Assert.Equal(1, adapter.DisposeCount);
    }

    [Fact]
    public void DisposeWhenRepeatedIsSafe()
    {
        var service = CreateService(out var adapter);

        service.Dispose();
        service.Dispose();

        Assert.Equal(1, adapter.DisposeCount);
        Assert.Equal(1, adapter.UnsubscriptionCount);
    }

    [Fact]
    public void CommandsAfterDisposeAreRejected()
    {
        var service = CreateService(out _);
        service.Dispose();

        Assert.Throws<ObjectDisposedException>(() => service.GetInstalledVoices());
        Assert.Throws<ObjectDisposedException>(() =>
        {
            _ = service.SpeakAsync("Hello", DefaultSettings);
        });
        Assert.Throws<ObjectDisposedException>(service.PauseSpeech);
        Assert.Throws<ObjectDisposedException>(service.ResumeSpeech);
        Assert.Throws<ObjectDisposedException>(service.StopSpeech);
    }

    [Fact]
    public void LateCompletionFromPreviousOperationIsIgnored()
    {
        using var service = CreateService(out var adapter);
        var states = ObserveStates(service);
        var firstTask = service.SpeakAsync("First", DefaultSettings);
        var firstToken = adapter.CurrentToken;
        adapter.Complete();
        Assert.True(firstTask.IsCompletedSuccessfully);
        _ = service.SpeakAsync("Second", DefaultSettings);

        adapter.Complete(token: firstToken);

        Assert.Equal(SpeechOperationState.Speaking, states[^1]);
    }

    private static TextToSpeechService CreateService(
        out FakeSpeechSynthesizerAdapter adapter)
    {
        adapter = new FakeSpeechSynthesizerAdapter();
        return new TextToSpeechService(adapter);
    }

    private static List<SpeechOperationState> ObserveStates(TextToSpeechService service)
    {
        var states = new List<SpeechOperationState>();
        service.StateChanged += (_, eventArgs) => states.Add(eventArgs.CurrentState);
        return states;
    }
}
