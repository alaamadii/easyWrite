using EasyWrite.Core.Models;
using EasyWrite.Infrastructure.TextToSpeech;

namespace EasyWrite.Infrastructure.Tests.TextToSpeech;

internal sealed class FakeSpeechSynthesizerAdapter : ISpeechSynthesizerAdapter
{
    private EventHandler<SynthesisCompletedEventArgs>? _speakCompleted;

    public event EventHandler<SynthesisCompletedEventArgs>? SpeakCompleted
    {
        add
        {
            _speakCompleted += value;
            SubscriptionCount++;
        }

        remove
        {
            _speakCompleted -= value;
            UnsubscriptionCount++;
        }
    }

    public List<SpeechVoice> Voices { get; } =
    [
        new SpeechVoice("Test Voice", "English (Test)", "Deterministic test voice", true)
    ];

    public int SubscriptionCount { get; private set; }

    public int UnsubscriptionCount { get; private set; }

    public string? SelectedVoice { get; private set; }

    public int Rate { get; private set; }

    public int Volume { get; private set; }

    public bool DefaultOutputSelected { get; private set; }

    public string? SpokenText { get; private set; }

    public object? CurrentToken { get; private set; }

    public int PauseCount { get; private set; }

    public int ResumeCount { get; private set; }

    public int CancelCount { get; private set; }

    public int DisposeCount { get; private set; }

    public IReadOnlyList<SpeechVoice> GetInstalledVoices() => Voices;

    public void SelectVoice(string voiceName) => SelectedVoice = voiceName;

    public void SetRate(int rate) => Rate = rate;

    public void SetVolume(int volume) => Volume = volume;

    public void SetOutputToDefaultAudioDevice() => DefaultOutputSelected = true;

    public object SpeakAsync(string text)
    {
        SpokenText = text;
        CurrentToken = new object();
        return CurrentToken;
    }

    public void Pause() => PauseCount++;

    public void Resume() => ResumeCount++;

    public void CancelAll() => CancelCount++;

    public void Dispose() => DisposeCount++;

    public void Complete(bool cancelled = false, Exception? error = null, object? token = null)
    {
        var completedToken = token ?? CurrentToken ?? new object();
        _speakCompleted?.Invoke(
            this,
            new SynthesisCompletedEventArgs(completedToken, cancelled, error));
    }
}
