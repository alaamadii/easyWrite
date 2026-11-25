using System.Globalization;
using System.Text;
using EasyWrite.Core.Models;

namespace EasyWrite.Core.Formatting;

public static class TranscriptFormatter
{
    public const string DocumentTitle = "EasyWrite Transcript";

    public static string FormatEntry(SpeechResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(result.Text))
        {
            throw new ArgumentException("Recognized text cannot be blank.", nameof(result));
        }

        var localTimestamp = result.Timestamp.ToLocalTime();
        return string.Create(
            CultureInfo.InvariantCulture,
            $"[{localTimestamp:HH:mm:ss}] {result.Text.Trim()}");
    }

    public static string FormatDocument(IReadOnlyCollection<SpeechResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        if (results.Count == 0)
        {
            throw new ArgumentException("At least one transcript result is required.", nameof(results));
        }

        var content = new StringBuilder(DocumentTitle)
            .AppendLine()
            .AppendLine();

        foreach (var result in results)
        {
            content.AppendLine(FormatEntry(result));
        }

        return content.ToString();
    }
}
