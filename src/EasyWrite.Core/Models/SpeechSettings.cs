namespace EasyWrite.Core.Models;

/// <summary>Settings applied to a text-to-speech operation.</summary>
public sealed record SpeechSettings
{
    public const int MinimumRate = -10;
    public const int MaximumRate = 10;
    public const int MinimumVolume = 0;
    public const int MaximumVolume = 100;

    public string? VoiceName { get; init; }

    public int Rate { get; init; }

    public int Volume { get; init; } = MaximumVolume;

    public void Validate()
    {
        if (Rate is < MinimumRate or > MaximumRate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Rate),
                $"Rate must be between {MinimumRate} and {MaximumRate}.");
        }

        if (Volume is < MinimumVolume or > MaximumVolume)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Volume),
                $"Volume must be between {MinimumVolume} and {MaximumVolume}.");
        }
    }
}
