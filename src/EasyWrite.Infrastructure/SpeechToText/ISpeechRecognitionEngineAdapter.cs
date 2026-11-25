namespace EasyWrite.Infrastructure.SpeechToText;

internal interface ISpeechRecognitionEngineAdapter : IDisposable
{
    event EventHandler<RecognitionResultEventArgs>? SpeechRecognized;

    event EventHandler<RecognitionCompletedEventArgs>? RecognitionCompleted;

    void LoadDictationGrammar();

    void SetInputToDefaultAudioDevice();

    void StartContinuousRecognition();

    void CancelRecognition();
}
