using EasyWrite.Core.Models;
using Xunit;

namespace EasyWrite.Core.Tests.Models;

public sealed class SpeechSettingsTests
{
    [Theory]
    [InlineData(SpeechSettings.MinimumRate, SpeechSettings.MinimumVolume)]
    [InlineData(0, 50)]
    [InlineData(SpeechSettings.MaximumRate, SpeechSettings.MaximumVolume)]
    public void ValidateAcceptsSupportedValues(int rate, int volume)
    {
        var settings = new SpeechSettings { Rate = rate, Volume = volume };

        settings.Validate();
    }

    [Theory]
    [InlineData(-11)]
    [InlineData(11)]
    public void ValidateRejectsUnsupportedRate(int rate)
    {
        var settings = new SpeechSettings { Rate = rate };

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void ValidateRejectsUnsupportedVolume(int volume)
    {
        var settings = new SpeechSettings { Volume = volume };

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }
}
