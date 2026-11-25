namespace EasyWrite.Infrastructure.Export;

internal interface ITranscriptFileWriter
{
    Task WriteAsync(string path, string content, CancellationToken cancellationToken);
}
