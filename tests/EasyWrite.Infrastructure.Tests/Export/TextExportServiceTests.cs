using System.Text;
using EasyWrite.Core.Formatting;
using EasyWrite.Core.Models;
using EasyWrite.Infrastructure.Export;
using Xunit;

namespace EasyWrite.Infrastructure.Tests.Export;

public sealed class TextExportServiceTests
{
    [Fact]
    public async Task ExportRejectsBlankDestination()
    {
        var service = new TextExportService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExportAsync("  ", CreateResults()));
    }

    [Fact]
    public async Task ExportRejectsEmptyTranscript()
    {
        using var directory = new TemporaryDirectory();
        var service = new TextExportService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExportAsync(directory.File("empty.txt"), []));
    }

    [Fact]
    public async Task ExportWritesUtf8WithoutByteOrderMarkAndDocumentFormat()
    {
        using var directory = new TemporaryDirectory();
        var destination = directory.File("transcript.txt");
        var results = CreateResults();
        var service = new TextExportService();

        await service.ExportAsync(destination, results);

        var bytes = await File.ReadAllBytesAsync(destination);
        Assert.False(bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble));
        Assert.Equal(TranscriptFormatter.FormatDocument(results), Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public async Task ExportPreservesUnicodeAndResultOrder()
    {
        using var directory = new TemporaryDirectory();
        var destination = directory.File("unicode.txt");
        SpeechResult[] results =
        [
            CreateResult("Türkçe metin", 1),
            CreateResult("نص عربي", 2)
        ];
        var service = new TextExportService();

        await service.ExportAsync(destination, results);

        var content = await File.ReadAllTextAsync(destination, Encoding.UTF8);
        Assert.True(content.IndexOf(results[0].Text, StringComparison.Ordinal) < content.IndexOf(results[1].Text, StringComparison.Ordinal));
    }

    [Fact]
    public async Task AlreadyCancelledExportCreatesNoFile()
    {
        using var directory = new TemporaryDirectory();
        var destination = directory.File("cancelled.txt");
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var service = new TextExportService();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ExportAsync(destination, CreateResults(), cancellationSource.Token));

        Assert.False(File.Exists(destination));
        Assert.Empty(Directory.EnumerateFiles(directory.Path));
    }

    [Fact]
    public async Task CancellationDuringWriteDeletesPartialTempAndPreservesDestination()
    {
        using var directory = new TemporaryDirectory();
        var destination = directory.File("existing.txt");
        await File.WriteAllTextAsync(destination, "original");
        var writer = new FakeTranscriptFileWriter
        {
            WriteAction = async (path, _, _) =>
            {
                await File.WriteAllTextAsync(path, "partial", CancellationToken.None);
                throw new OperationCanceledException();
            }
        };
        var service = new TextExportService(writer);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ExportAsync(destination, CreateResults()));

        Assert.Equal("original", await File.ReadAllTextAsync(destination));
        Assert.Equal([destination], Directory.EnumerateFiles(directory.Path).ToArray());
    }

    [Fact]
    public async Task SuccessfulExportReplacesExistingApprovedFile()
    {
        using var directory = new TemporaryDirectory();
        var destination = directory.File("existing.txt");
        await File.WriteAllTextAsync(destination, "old content");
        var service = new TextExportService();

        await service.ExportAsync(destination, CreateResults());

        Assert.DoesNotContain("old content", await File.ReadAllTextAsync(destination), StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriteFailureDeletesTempPreservesExistingFileAndInnerException()
    {
        using var directory = new TemporaryDirectory();
        var destination = directory.File("existing.txt");
        await File.WriteAllTextAsync(destination, "original");
        var sourceError = new IOException("Disk write failed");
        var writer = new FakeTranscriptFileWriter
        {
            WriteAction = async (path, _, _) =>
            {
                await File.WriteAllTextAsync(path, "partial", CancellationToken.None);
                throw sourceError;
            }
        };
        var service = new TextExportService(writer);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExportAsync(destination, CreateResults()));

        Assert.Same(sourceError, exception.InnerException);
        Assert.Equal("original", await File.ReadAllTextAsync(destination));
        Assert.Equal([destination], Directory.EnumerateFiles(directory.Path).ToArray());
    }

    [Fact]
    public async Task ExportCanRecoverAfterWriteFailure()
    {
        using var directory = new TemporaryDirectory();
        var failedDestination = directory.File("failed.txt");
        var recoveredDestination = directory.File("recovered.txt");
        var attempt = 0;
        var writer = new FakeTranscriptFileWriter
        {
            WriteAction = async (path, content, cancellationToken) =>
            {
                attempt++;
                if (attempt == 1)
                {
                    await File.WriteAllTextAsync(path, "partial", CancellationToken.None);
                    throw new IOException("First write failed");
                }

                await File.WriteAllTextAsync(
                    path,
                    content,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    cancellationToken);
            }
        };
        var service = new TextExportService(writer);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExportAsync(failedDestination, CreateResults()));
        await service.ExportAsync(recoveredDestination, CreateResults());

        Assert.False(File.Exists(failedDestination));
        Assert.True(File.Exists(recoveredDestination));
        Assert.Equal([recoveredDestination], Directory.EnumerateFiles(directory.Path).ToArray());
    }

    [Fact]
    public async Task InvalidDirectoryPreservesUnderlyingException()
    {
        using var directory = new TemporaryDirectory();
        var destination = System.IO.Path.Combine(directory.Path, "missing", "transcript.txt");
        var service = new TextExportService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExportAsync(destination, CreateResults()));

        Assert.IsType<DirectoryNotFoundException>(exception.InnerException);
    }

    [Fact]
    public async Task SuccessfulExportReleasesFileResources()
    {
        using var directory = new TemporaryDirectory();
        var destination = directory.File("released.txt");
        var service = new TextExportService();
        await service.ExportAsync(destination, CreateResults());

        using var stream = new FileStream(destination, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        Assert.True(stream.CanWrite);
    }

    [Fact]
    public async Task MultipleSequentialExportsSucceed()
    {
        using var directory = new TemporaryDirectory();
        var service = new TextExportService();

        for (var export = 0; export < 10; export++)
        {
            await service.ExportAsync(directory.File($"transcript-{export}.txt"), CreateResults());
        }

        Assert.Equal(10, Directory.EnumerateFiles(directory.Path).Count());
    }

    private static SpeechResult[] CreateResults() =>
    [
        CreateResult("first result", 1),
        CreateResult("second result", 2)
    ];

    private static SpeechResult CreateResult(string text, int second) => new(
        text,
        new DateTimeOffset(2026, 11, 25, 12, 0, second, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)),
        0.8F);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "EasyWrite.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string File(string name) => System.IO.Path.Combine(Path, name);

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
