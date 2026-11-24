using System.Speech.Synthesis;
using EasyWrite.Core.Models;

namespace EasyWrite.Infrastructure.TextToSpeech;

internal sealed class SystemSpeechSynthesizerAdapter : ISpeechSynthesizerAdapter
{
    private readonly SpeechSynthesizer _synthesizer = new();
    private bool _disposed;

    public SystemSpeechSynthesizerAdapter()
    {
        _synthesizer.SpeakCompleted += HandleSpeakCompleted;
    }

    public event EventHandler<SynthesisCompletedEventArgs>? SpeakCompleted;

    public IReadOnlyList<SpeechVoice> GetInstalledVoices()
    {
        ThrowIfDisposed();

        // The parameterless overload is intentional: it returns every installed
        // Windows voice instead of restricting discovery to one culture.
#pragma warning disable CA1304
        var installedVoices = _synthesizer.GetInstalledVoices();
#pragma warning restore CA1304

        return installedVoices
            .Where(voice =>
                voice.Enabled &&
                voice.VoiceInfo is not null &&
                !string.IsNullOrWhiteSpace(voice.VoiceInfo.Name))
            .Select(MapVoice)
            .OrderBy(voice => voice.Culture, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(voice => voice.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public void SelectVoice(string voiceName)
    {
        ThrowIfDisposed();
        _synthesizer.SelectVoice(voiceName);
    }

    public void SetRate(int rate)
    {
        ThrowIfDisposed();
        _synthesizer.Rate = rate;
    }

    public void SetVolume(int volume)
    {
        ThrowIfDisposed();
        _synthesizer.Volume = volume;
    }

    public void SetOutputToDefaultAudioDevice()
    {
        ThrowIfDisposed();
        _synthesizer.SetOutputToDefaultAudioDevice();
    }

    public object SpeakAsync(string text)
    {
        ThrowIfDisposed();
        return _synthesizer.SpeakAsync(text);
    }

    public void Pause()
    {
        ThrowIfDisposed();
        _synthesizer.Pause();
    }

    public void Resume()
    {
        ThrowIfDisposed();
        _synthesizer.Resume();
    }

    public void CancelAll()
    {
        ThrowIfDisposed();
        _synthesizer.SpeakAsyncCancelAll();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _synthesizer.SpeakCompleted -= HandleSpeakCompleted;
        _synthesizer.Dispose();
    }

    private void HandleSpeakCompleted(object? sender, SpeakCompletedEventArgs e)
    {
        SpeakCompleted?.Invoke(
            this,
            new SynthesisCompletedEventArgs(e.Prompt, e.Cancelled, e.Error));
    }

    private static SpeechVoice MapVoice(InstalledVoice installedVoice)
    {
        var voiceInfo = installedVoice.VoiceInfo!;
        return new SpeechVoice(
            voiceInfo.Name!,
            voiceInfo.Culture?.DisplayName ?? "Unknown culture",
            voiceInfo.Description ?? voiceInfo.Name!,
            installedVoice.Enabled);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
