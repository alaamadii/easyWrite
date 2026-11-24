using System.Speech.Recognition;

namespace EasyWrite.Infrastructure.SpeechToText;

internal sealed class SystemSpeechRecognitionEngineFactory : ISpeechRecognitionEngineFactory
{
    public IReadOnlyList<RecognizerDescriptor> GetInstalledRecognizers() =>
        SpeechRecognitionEngine.InstalledRecognizers()
            .Where(recognizer =>
                !string.IsNullOrWhiteSpace(recognizer.Id) &&
                recognizer.Culture is not null)
            .Select(recognizer => new RecognizerDescriptor(
                recognizer.Id,
                string.IsNullOrWhiteSpace(recognizer.Name)
                    ? recognizer.Culture.DisplayName
                    : recognizer.Name,
                recognizer.Culture,
                string.IsNullOrWhiteSpace(recognizer.Description)
                    ? recognizer.Culture.DisplayName
                    : recognizer.Description))
            .OrderBy(
                recognizer => recognizer.Culture.DisplayName,
                StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(recognizer => recognizer.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

    public ISpeechRecognitionEngineAdapter Create(string recognizerId) =>
        new SystemSpeechRecognitionEngineAdapter(recognizerId);
}
