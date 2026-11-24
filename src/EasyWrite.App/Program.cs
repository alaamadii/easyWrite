using EasyWrite.Core.Services;
using EasyWrite.Infrastructure.SpeechToText;
using EasyWrite.Infrastructure.TextToSpeech;

namespace EasyWrite.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        ITextToSpeechService? textToSpeechService = null;
        ISpeechToTextService? speechToTextService = null;
        string? synthesisInitializationError = null;
        string? recognitionInitializationError = null;
        try
        {
            textToSpeechService = new TextToSpeechService();
        }
        catch (Exception exception)
        {
            synthesisInitializationError =
                "Windows speech could not be initialized. Check Windows speech settings and restart EasyWrite.";
            System.Diagnostics.Debug.WriteLine(exception);
        }

        try
        {
            speechToTextService = new SpeechToTextService();
        }
        catch (Exception exception)
        {
            recognitionInitializationError =
                "Windows speech recognition could not be initialized. Check Windows speech-language settings and restart EasyWrite.";
            System.Diagnostics.Debug.WriteLine(exception);
        }

        using (textToSpeechService)
        using (speechToTextService)
        {
            Application.Run(new MainForm(
                textToSpeechService,
                speechToTextService,
                synthesisInitializationError,
                recognitionInitializationError));
        }
    }
}
