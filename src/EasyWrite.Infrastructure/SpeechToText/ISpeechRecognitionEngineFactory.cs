namespace EasyWrite.Infrastructure.SpeechToText;

internal interface ISpeechRecognitionEngineFactory
{
    IReadOnlyList<RecognizerDescriptor> GetInstalledRecognizers();

    ISpeechRecognitionEngineAdapter Create(string recognizerId);
}
