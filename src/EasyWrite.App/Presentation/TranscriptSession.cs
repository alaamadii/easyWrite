using EasyWrite.Core.Models;

namespace EasyWrite.App.Presentation;

/// <summary>Owns finalized results displayed and exported during the current app session.</summary>
public sealed class TranscriptSession
{
    private readonly List<SpeechResult> _results = [];

    public int Count => _results.Count;

    public bool HasResults => _results.Count > 0;

    public void Add(SpeechResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(result.Text))
        {
            throw new ArgumentException("Recognized text cannot be blank.", nameof(result));
        }

        _results.Add(result);
    }

    public void Clear() => _results.Clear();

    public IReadOnlyList<SpeechResult> CreateSnapshot() => _results.ToArray();
}
