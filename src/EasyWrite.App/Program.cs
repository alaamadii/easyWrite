using EasyWrite.Core.Services;
using EasyWrite.Infrastructure.Export;
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
        ITextExportService? textExportService = null;
        string? synthesisInitializationError = null;
        string? recognitionInitializationError = null;
        string? exportInitializationError = null;
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
            textExportService = new TextExportService();
        }
        catch (Exception exception)
        {
            exportInitializationError =
                "Transcript export could not be initialized. Restart EasyWrite and try again.";
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
                textExportService,
                synthesisInitializationError,
                recognitionInitializationError,
                exportInitializationError));
        }
    }
}
