using System.Globalization;
using EasyWrite.Core.Events;
using EasyWrite.Core.Models;

namespace EasyWrite.Core.Services;

public interface ISpeechToTextService : IDisposable
{
    event EventHandler<SpeechResultEventArgs>? ResultRecognized;

    event EventHandler<SpeechStateChangedEventArgs>? StateChanged;

    IReadOnlyList<CultureInfo> GetInstalledRecognizerCultures();

    Task StartListeningAsync(CultureInfo culture, CancellationToken cancellationToken = default);

    Task StopListeningAsync(CancellationToken cancellationToken = default);
}
