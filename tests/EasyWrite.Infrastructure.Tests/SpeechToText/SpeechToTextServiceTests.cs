using System.Globalization;
using EasyWrite.Core.Models;
using EasyWrite.Infrastructure.SpeechToText;
using Xunit;

namespace EasyWrite.Infrastructure.Tests.SpeechToText;

public sealed class SpeechToTextServiceTests
{
    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");

    [Fact]
    public void GetInstalledRecognizerCulturesReturnsDistinctStableCultures()
    {
        var factory = new FakeSpeechRecognitionEngineFactory();
        factory.Recognizers.Clear();
        factory.Recognizers.Add(CreateRecognizer("tr", "tr-TR", "Turkish"));
        factory.Recognizers.Add(CreateRecognizer("en-2", "en-US", "English Two"));
        factory.Recognizers.Add(CreateRecognizer("en-1", "en-US", "English One"));
        factory.Recognizers.Add(new RecognizerDescriptor(
            string.Empty,
            "Invalid",
            CultureInfo.InvariantCulture,
            "Invalid"));
        using var service = new SpeechToTextService(factory);

        var cultures = service.GetInstalledRecognizerCultures();

        Assert.Equal(2, cultures.Count);
        Assert.Contains(cultures, culture => culture.Name == "en-US");
        Assert.Contains(cultures, culture => culture.Name == "tr-TR");
    }

    [Fact]
    public void GetInstalledRecognizerCulturesWhenNoneExistReturnsEmptyCollection()
    {
        var factory = new FakeSpeechRecognitionEngineFactory();
        factory.Recognizers.Clear();
        using var service = new SpeechToTextService(factory);

        Assert.Empty(service.GetInstalledRecognizerCultures());
    }

    [Fact]
    public void DiscoveryFailurePreservesUnderlyingException()
    {
        var factory = new FakeSpeechRecognitionEngineFactory
        {
            EnumerationException = new InvalidOperationException("Registry unavailable")
        };
        using var service = new SpeechToTextService(factory);

        var exception = Assert.Throws<InvalidOperationException>(
            service.GetInstalledRecognizerCultures);

        Assert.Same(factory.EnumerationException, exception.InnerException);
    }

    [Fact]
    public async Task StartListeningConfiguresSelectedRecognizerGrammarInputAndContinuousMode()
    {
        using var service = CreateService(out var factory);

        await service.StartListeningAsync(EnglishCulture);

        var engine = Assert.Single(factory.CreatedEngines);
        Assert.Equal("test-en-US", factory.CreatedRecognizerId);
        Assert.True(engine.DictationGrammarLoaded);
        Assert.True(engine.DefaultInputSelected);
        Assert.Equal(1, engine.StartCount);
    }

    [Fact]
    public async Task StartListeningPublishesListening()
    {
        using var service = CreateService(out _);
        var states = ObserveStates(service);

        await service.StartListeningAsync(EnglishCulture);

        Assert.Equal([SpeechOperationState.Listening], states);
    }

    [Fact]
    public async Task FinalResultIsTrimmedClampedTimestampedAndPublished()
    {
        using var service = CreateService(out var factory);
        var results = ObserveResults(service);
        var earliestTimestamp = DateTimeOffset.Now;
        await service.StartListeningAsync(EnglishCulture);

        factory.CreatedEngines[0].EmitResult("  hello EasyWrite  ", 1.4F);

        var result = Assert.Single(results);
        Assert.Equal("hello EasyWrite", result.Text);
        Assert.Equal(1F, result.Confidence);
        Assert.InRange(result.Timestamp, earliestTimestamp, DateTimeOffset.Now);
    }

    [Fact]
    public async Task EmptyRecognitionIsIgnored()
    {
        using var service = CreateService(out var factory);
        var results = ObserveResults(service);
        await service.StartListeningAsync(EnglishCulture);

        factory.CreatedEngines[0].EmitResult("   ");

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleFinalResultsPreserveReceivedOrder()
    {
        using var service = CreateService(out var factory);
        var results = ObserveResults(service);
        await service.StartListeningAsync(EnglishCulture);

        factory.CreatedEngines[0].EmitResult("first");
        factory.CreatedEngines[0].EmitResult("second");
        factory.CreatedEngines[0].EmitResult("third");

        Assert.Equal(["first", "second", "third"], results.Select(result => result.Text));
    }

    [Fact]
    public async Task StopListeningPublishesStoppingThenIdleAndCompletes()
    {
        using var service = CreateService(out var factory);
        var states = ObserveStates(service);
        await service.StartListeningAsync(EnglishCulture);

        var stopTask = service.StopListeningAsync();
        factory.CreatedEngines[0].Complete();
        await stopTask;

        Assert.Equal(1, factory.CreatedEngines[0].CancelCount);
        Assert.Equal(
            [
                SpeechOperationState.Listening,
                SpeechOperationState.Stopping,
                SpeechOperationState.Idle
            ],
            states);
    }

    [Fact]
    public async Task RepeatedStopUsesOneCancellationRequest()
    {
        using var service = CreateService(out var factory);
        await service.StartListeningAsync(EnglishCulture);

        var firstStop = service.StopListeningAsync();
        var secondStop = service.StopListeningAsync();
        factory.CreatedEngines[0].Complete();
        await Task.WhenAll(firstStop, secondStop);

        Assert.Equal(1, factory.CreatedEngines[0].CancelCount);
    }

    [Fact]
    public async Task ReentrantStartIsRejected()
    {
        using var service = CreateService(out _);
        await service.StartListeningAsync(EnglishCulture);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartListeningAsync(EnglishCulture));

        Assert.Contains("already active", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancellationRequestsSafeStopAndReturnsToIdle()
    {
        using var service = CreateService(out var factory);
        using var cancellationSource = new CancellationTokenSource();
        var states = ObserveStates(service);
        await service.StartListeningAsync(EnglishCulture, cancellationSource.Token);

        cancellationSource.Cancel();
        factory.CreatedEngines[0].Complete();

        Assert.Equal(1, factory.CreatedEngines[0].CancelCount);
        Assert.Equal(SpeechOperationState.Idle, states[^1]);
    }

    [Fact]
    public async Task AlreadyCancelledStartDoesNotCreateEngine()
    {
        using var service = CreateService(out var factory);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.StartListeningAsync(EnglishCulture, cancellationSource.Token));

        Assert.Empty(factory.CreatedEngines);
    }

    [Fact]
    public async Task StartupFailurePublishesErrorAndPreservesInnerException()
    {
        var sourceError = new InvalidOperationException("No default audio input");
        var factory = new FakeSpeechRecognitionEngineFactory
        {
            ConfigureEngine = engine => engine.InputException = sourceError
        };
        using var service = new SpeechToTextService(factory);
        var states = ObserveStates(service);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartListeningAsync(EnglishCulture));

        Assert.Same(sourceError, exception.InnerException);
        Assert.Equal(SpeechOperationState.Error, states[^1]);
        Assert.Equal(1, factory.CreatedEngines[0].DisposeCount);
    }

    [Fact]
    public async Task RuntimeFailurePublishesErrorAndFaultsPendingStop()
    {
        using var service = CreateService(out var factory);
        var states = ObserveStates(service);
        await service.StartListeningAsync(EnglishCulture);
        var stopTask = service.StopListeningAsync();
        var sourceError = new InvalidOperationException("Recognizer failed");

        factory.CreatedEngines[0].Complete(error: sourceError);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => stopTask);
        Assert.Same(sourceError, exception.InnerException);
        Assert.Equal(SpeechOperationState.Error, states[^1]);
    }

    [Fact]
    public async Task UnavailableSelectedCultureIsRejected()
    {
        using var service = CreateService(out var factory);
        var states = ObserveStates(service);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartListeningAsync(CultureInfo.GetCultureInfo("tr-TR")));

        Assert.Contains("no longer available", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(factory.CreatedEngines);
        Assert.Equal([SpeechOperationState.Error], states);
    }

    [Fact]
    public async Task StartWithNoInstalledRecognizerPublishesError()
    {
        var factory = new FakeSpeechRecognitionEngineFactory();
        factory.Recognizers.Clear();
        using var service = new SpeechToTextService(factory);
        var states = ObserveStates(service);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartListeningAsync(EnglishCulture));

        Assert.Contains("No compatible", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal([SpeechOperationState.Error], states);
    }

    [Fact]
    public async Task StopWhileIdleCompletesWithoutCreatingEngine()
    {
        using var service = CreateService(out var factory);

        await service.StopListeningAsync();

        Assert.Empty(factory.CreatedEngines);
    }

    [Fact]
    public async Task StopFailurePublishesErrorAndPreservesInnerException()
    {
        var sourceError = new InvalidOperationException("Cancellation failed");
        var factory = new FakeSpeechRecognitionEngineFactory
        {
            ConfigureEngine = engine => engine.CancelException = sourceError
        };
        using var service = new SpeechToTextService(factory);
        var states = ObserveStates(service);
        await service.StartListeningAsync(EnglishCulture);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StopListeningAsync());

        Assert.Same(sourceError, exception.InnerException);
        Assert.Equal(SpeechOperationState.Error, states[^1]);
        Assert.Equal(1, factory.CreatedEngines[0].DisposeCount);
    }

    [Fact]
    public async Task LateEventsFromPreviousSessionAreIgnored()
    {
        using var service = CreateService(out var factory);
        var states = ObserveStates(service);
        var results = ObserveResults(service);
        await service.StartListeningAsync(EnglishCulture);
        var oldEngine = factory.CreatedEngines[0];
        var stopTask = service.StopListeningAsync();
        oldEngine.Complete();
        await stopTask;
        await service.StartListeningAsync(EnglishCulture);

        oldEngine.EmitResult("late result");
        oldEngine.Complete(error: new InvalidOperationException("late error"));

        Assert.Empty(results);
        Assert.Equal(SpeechOperationState.Listening, states[^1]);
    }

    [Fact]
    public async Task DisposeCancelsActiveEngineAndUnsubscribesEvents()
    {
        var service = CreateService(out var factory);
        await service.StartListeningAsync(EnglishCulture);
        var engine = factory.CreatedEngines[0];

        service.Dispose();

        Assert.Equal(1, engine.CancelCount);
        Assert.Equal(1, engine.ResultUnsubscriptionCount);
        Assert.Equal(1, engine.CompletionUnsubscriptionCount);
        Assert.Equal(1, engine.DisposeCount);
    }

    [Fact]
    public void RepeatedDisposeIsSafe()
    {
        var service = CreateService(out _);

        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public async Task CommandsAfterDisposeAreRejected()
    {
        var service = CreateService(out _);
        service.Dispose();

        Assert.Throws<ObjectDisposedException>(service.GetInstalledRecognizerCultures);
        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => service.StartListeningAsync(EnglishCulture));
        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => service.StopListeningAsync());
    }

    [Fact]
    public async Task TenStartStopCyclesReleaseEveryEngineWithoutDuplicateCallbacks()
    {
        using var service = CreateService(out var factory);
        var states = ObserveStates(service);
        var results = ObserveResults(service);

        for (var cycle = 0; cycle < 10; cycle++)
        {
            await service.StartListeningAsync(EnglishCulture);
            var engine = factory.CreatedEngines[cycle];
            engine.EmitResult($"cycle {cycle}");
            var stopTask = service.StopListeningAsync();
            engine.Complete();
            await stopTask;

            Assert.Equal(1, engine.CancelCount);
            Assert.Equal(1, engine.ResultUnsubscriptionCount);
            Assert.Equal(1, engine.CompletionUnsubscriptionCount);
            Assert.Equal(1, engine.DisposeCount);
            Assert.Equal(SpeechOperationState.Idle, states[^1]);
        }

        Assert.Equal(10, factory.CreatedEngines.Count);
        Assert.Equal(10, results.Count);
        Assert.Equal(30, states.Count);
    }

    [Fact]
    public async Task DisposeWhileStoppingCancelsPendingStopAndIgnoresLateCallbacks()
    {
        var service = CreateService(out var factory);
        var states = ObserveStates(service);
        var results = ObserveResults(service);
        await service.StartListeningAsync(EnglishCulture);
        var engine = factory.CreatedEngines[0];
        var stopTask = service.StopListeningAsync();

        service.Dispose();
        var stateCountAfterDispose = states.Count;
        engine.EmitResult("late result");
        engine.Complete(error: new InvalidOperationException("late failure"));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stopTask);
        Assert.Empty(results);
        Assert.Equal(stateCountAfterDispose, states.Count);
        Assert.Equal(1, engine.ResultUnsubscriptionCount);
        Assert.Equal(1, engine.CompletionUnsubscriptionCount);
        Assert.Equal(1, engine.DisposeCount);
    }

    private static SpeechToTextService CreateService(
        out FakeSpeechRecognitionEngineFactory factory)
    {
        factory = new FakeSpeechRecognitionEngineFactory();
        return new SpeechToTextService(factory);
    }

    private static RecognizerDescriptor CreateRecognizer(
        string id,
        string cultureName,
        string name) => new(
            id,
            name,
            CultureInfo.GetCultureInfo(cultureName),
            name);

    private static List<SpeechOperationState> ObserveStates(SpeechToTextService service)
    {
        var states = new List<SpeechOperationState>();
        service.StateChanged += (_, eventArgs) => states.Add(eventArgs.CurrentState);
        return states;
    }

    private static List<SpeechResult> ObserveResults(SpeechToTextService service)
    {
        var results = new List<SpeechResult>();
        service.ResultRecognized += (_, eventArgs) => results.Add(eventArgs.Result);
        return results;
    }
}
