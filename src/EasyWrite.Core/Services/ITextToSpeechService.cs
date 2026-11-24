using EasyWrite.Core.Events;
using EasyWrite.Core.Models;

namespace EasyWrite.Core.Services;

public interface ITextToSpeechService : IDisposable
{
    event EventHandler<SpeechStateChangedEventArgs>? StateChanged;

    IReadOnlyList<SpeechVoice> GetInstalledVoices();

    Task SpeakAsync(string text, SpeechSettings settings, CancellationToken cancellationToken = default);

    void PauseSpeech();

    void ResumeSpeech();

    void StopSpeech();
}
