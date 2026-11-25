using EasyWrite.Core.Formatting;
using EasyWrite.Core.Models;
using Xunit;

namespace EasyWrite.Core.Tests.Formatting;

public sealed class TranscriptFormatterTests
{
    [Fact]
    public void FormatEntryUsesLocalTimeAndStableClockFormat()
    {
        var localTime = new DateTime(2026, 11, 25, 14, 32, 8, DateTimeKind.Unspecified);
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(localTime);
        var result = new SpeechResult(
            "Recognized words",
            new DateTimeOffset(localTime, localOffset),
            0.75F);

        var formatted = TranscriptFormatter.FormatEntry(result);

        Assert.Equal("[14:32:08] Recognized words", formatted);
    }

    [Fact]
    public void FormatEntryConvertsTimestampToMachineLocalTime()
    {
        var timestamp = new DateTimeOffset(2026, 11, 25, 9, 15, 30, TimeSpan.Zero);
        var expectedTime = timestamp.ToLocalTime().ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

        var formatted = TranscriptFormatter.FormatEntry(
            new SpeechResult("Hello", timestamp, 0.9F));

        Assert.Equal($"[{expectedTime}] Hello", formatted);
    }

    [Fact]
    public void FormatEntryRejectsBlankText()
    {
        var result = new SpeechResult("   ", DateTimeOffset.Now, 0.5F);

        Assert.Throws<ArgumentException>(() => TranscriptFormatter.FormatEntry(result));
    }

    [Fact]
    public void FormatDocumentPreservesOrderAndUnicode()
    {
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        SpeechResult[] results =
        [
            new("Merhaba", new DateTimeOffset(2026, 11, 25, 10, 0, 1, localOffset), 0.8F),
            new("مرحباً", new DateTimeOffset(2026, 11, 25, 10, 0, 2, localOffset), 0.7F)
        ];

        var document = TranscriptFormatter.FormatDocument(results);

        Assert.StartsWith($"{TranscriptFormatter.DocumentTitle}{Environment.NewLine}{Environment.NewLine}", document, StringComparison.Ordinal);
        Assert.True(document.IndexOf("Merhaba", StringComparison.Ordinal) < document.IndexOf("مرحباً", StringComparison.Ordinal));
        Assert.EndsWith(Environment.NewLine, document, StringComparison.Ordinal);
    }

    [Fact]
    public void FormattingDoesNotMutateResult()
    {
        var result = new SpeechResult("  original text  ", DateTimeOffset.Now, 0.42F);

        _ = TranscriptFormatter.FormatEntry(result);

        Assert.Equal("  original text  ", result.Text);
        Assert.Equal(0.42F, result.Confidence);
    }
}
