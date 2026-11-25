using EasyWrite.Core.Formatting;
using EasyWrite.Core.Models;
using EasyWrite.Core.Services;

namespace EasyWrite.Infrastructure.Export;

public sealed class TextExportService : ITextExportService
{
    private readonly ITranscriptFileWriter _fileWriter;

    public TextExportService()
        : this(new SystemTranscriptFileWriter())
    {
    }

    internal TextExportService(ITranscriptFileWriter fileWriter)
    {
        _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
    }

    public async Task ExportAsync(
        string destinationPath,
        IReadOnlyCollection<SpeechResult> results,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(results);
        cancellationToken.ThrowIfCancellationRequested();

        var content = TranscriptFormatter.FormatDocument(results);
        var fullDestinationPath = ResolveDestinationPath(destinationPath);
        var temporaryPath = $"{fullDestinationPath}.{Guid.NewGuid():N}.tmp";

        try
        {
            await _fileWriter.WriteAsync(temporaryPath, content, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, fullDestinationPath, overwrite: true);
        }
        catch (OperationCanceledException)
        {
            TryDeleteTemporaryFile(temporaryPath);
            throw;
        }
        catch (Exception exception) when (exception is
            ArgumentException or
            IOException or
            UnauthorizedAccessException or
            NotSupportedException or
            System.Security.SecurityException)
        {
            TryDeleteTemporaryFile(temporaryPath);
            throw new InvalidOperationException(
                "The transcript could not be saved. Choose a writable location and try again.",
                exception);
        }
    }

    private static string ResolveDestinationPath(string destinationPath)
    {
        try
        {
            return Path.GetFullPath(destinationPath);
        }
        catch (Exception exception) when (exception is
            IOException or
            UnauthorizedAccessException or
            ArgumentException or
            NotSupportedException or
            System.Security.SecurityException)
        {
            throw new InvalidOperationException(
                "The selected export path is invalid or unavailable. Choose another location.",
                exception);
        }
    }

    private static void TryDeleteTemporaryFile(string temporaryPath)
    {
        try
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Preserve the original export failure; a later cleanup can remove the temp file.
        }
    }
}
