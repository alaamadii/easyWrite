using EasyWrite.Core.Services;
using EasyWrite.Infrastructure.TextToSpeech;

namespace EasyWrite.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        ITextToSpeechService? textToSpeechService = null;
        string? initializationError = null;
        try
        {
            textToSpeechService = new TextToSpeechService();
        }
        catch (Exception exception)
        {
            initializationError =
                "Windows speech could not be initialized. Check Windows speech settings and restart EasyWrite.";
            System.Diagnostics.Debug.WriteLine(exception);
        }

        using (textToSpeechService)
        {
            Application.Run(new MainForm(textToSpeechService, initializationError));
        }
    }
}
