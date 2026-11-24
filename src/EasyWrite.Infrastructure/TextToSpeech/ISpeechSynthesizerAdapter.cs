using EasyWrite.Core.Models;

namespace EasyWrite.Infrastructure.TextToSpeech;

internal interface ISpeechSynthesizerAdapter : IDisposable
{
    event EventHandler<SynthesisCompletedEventArgs>? SpeakCompleted;

    IReadOnlyList<SpeechVoice> GetInstalledVoices();

    void SelectVoice(string voiceName);

    void SetRate(int rate);

    void SetVolume(int volume);

    void SetOutputToDefaultAudioDevice();

    object SpeakAsync(string text);

    void Pause();

    void Resume();

    void CancelAll();
}
