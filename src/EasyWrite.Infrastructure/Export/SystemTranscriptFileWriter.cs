using System.Text;

namespace EasyWrite.Infrastructure.Export;

internal sealed class SystemTranscriptFileWriter : ITranscriptFileWriter
{
    private static readonly UTF8Encoding Utf8WithoutByteOrderMark = new(false);

    public async Task WriteAsync(
        string path,
        string content,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var writer = new StreamWriter(stream, Utf8WithoutByteOrderMark);
        await writer.WriteAsync(content.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }
}
