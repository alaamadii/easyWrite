using EasyWrite.App.Presentation;
using EasyWrite.Core.Models;
using Xunit;

namespace EasyWrite.App.Tests.Presentation;

public sealed class TranscriptSessionTests
{
    [Fact]
    public void AddPreservesFinalizedResultOrder()
    {
        var session = new TranscriptSession();
        var first = CreateResult("first");
        var second = CreateResult("second");

        session.Add(first);
        session.Add(second);

        Assert.Equal([first, second], session.CreateSnapshot());
        Assert.Equal(2, session.Count);
        Assert.True(session.HasResults);
    }

    [Fact]
    public void AddRejectsBlankResult()
    {
        var session = new TranscriptSession();

        Assert.Throws<ArgumentException>(() => session.Add(CreateResult("  ")));
        Assert.False(session.HasResults);
    }

    [Fact]
    public void ClearRemovesAllResults()
    {
        var session = new TranscriptSession();
        session.Add(CreateResult("text"));

        session.Clear();

        Assert.Empty(session.CreateSnapshot());
        Assert.Equal(0, session.Count);
        Assert.False(session.HasResults);
    }

    [Fact]
    public void SnapshotIsNotChangedByLaterResults()
    {
        var session = new TranscriptSession();
        session.Add(CreateResult("first"));
        var snapshot = session.CreateSnapshot();

        session.Add(CreateResult("second"));

        Assert.Single(snapshot);
        Assert.Equal(2, session.Count);
    }

    private static SpeechResult CreateResult(string text) =>
        new(text, DateTimeOffset.Now, 0.8F);
}
