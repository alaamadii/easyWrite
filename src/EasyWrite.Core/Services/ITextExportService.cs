using EasyWrite.Core.Models;

namespace EasyWrite.Core.Services;

public interface ITextExportService
{
    Task ExportAsync(
        string destinationPath,
        IReadOnlyCollection<SpeechResult> results,
        CancellationToken cancellationToken = default);
}
