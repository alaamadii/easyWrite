using EasyWrite.Infrastructure.Export;

namespace EasyWrite.Infrastructure.Tests.Export;

internal sealed class FakeTranscriptFileWriter : ITranscriptFileWriter
{
    public Func<string, string, CancellationToken, Task>? WriteAction { get; set; }

    public Task WriteAsync(string path, string content, CancellationToken cancellationToken) =>
        WriteAction?.Invoke(path, content, cancellationToken) ?? Task.CompletedTask;
}
