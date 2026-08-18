#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using Lumina.Infrastructure.Core.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Security;

/// <summary>
/// Contains unit tests for the <see cref="FileHashService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileHashServiceTests
{
    private readonly FileHashService _sut = new();
    private readonly HashedFileSystemFileDtoFixture _hashedFileSystemFileDtoFixture = new();

    [Fact]
    public async Task HashFilesAsync_WhenCalledWithFiles_ShouldInvokeCallbackAndComputeHashes()
    {
        // Arrange
        using TemporaryTestDirectory temporaryTestDirectory = new();
        string filePath1 = temporaryTestDirectory.CreateFile("book1.pdf", "content of the first book");
        string filePath2 = temporaryTestDirectory.CreateFile("book2.pdf", "content of the second book");
        List<HashedFileSystemFileDto> inputFiles =
        [
            _hashedFileSystemFileDtoFixture.Create(path: filePath1, size: new FileInfo(filePath1).Length, currentHash: 0, oldHash: 0, ticks: 0),
            _hashedFileSystemFileDtoFixture.Create(path: filePath2, size: new FileInfo(filePath2).Length, currentHash: 0, oldHash: 0, ticks: 0)
        ];
        int callbackInvocationCount = 0;

        // Act
        List<HashedFileSystemFileDto> result = await _sut.HashFilesAsync(
            inputFiles,
            () =>
            {
                callbackInvocationCount++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2, callbackInvocationCount);
        Assert.All(result, file => Assert.NotEqual(0UL, file.CurrentHash));
        // the hashing is parallelized, so the results are not guaranteed to be in the input order
        Assert.Equal(
            inputFiles.Select(file => file.Path).OrderBy(path => path, StringComparer.Ordinal),
            result.Select(file => file.Path).OrderBy(path => path, StringComparer.Ordinal));
    }

    [Fact]
    public async Task HashFilesAsync_WhenFileIsEmpty_ShouldUseEmptyFileHash()
    {
        // Arrange
        using TemporaryTestDirectory temporaryTestDirectory = new();
        string filePath = temporaryTestDirectory.CreateFile("empty.pdf", string.Empty);
        List<HashedFileSystemFileDto> inputFiles = [_hashedFileSystemFileDtoFixture.Create(path: filePath, size: new FileInfo(filePath).Length, currentHash: 0, oldHash: 0, ticks: 0)];

        // Act
        List<HashedFileSystemFileDto> result = await _sut.HashFilesAsync(inputFiles, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        HashedFileSystemFileDto hashedFile = Assert.Single(result);
        Assert.Equal(filePath, hashedFile.Path);
        Assert.Equal(BitConverter.ToUInt64(new XxHash64().GetCurrentHash()), hashedFile.CurrentHash);
    }

    [Fact]
    public async Task HashFilesAsync_WhenFileContentChanges_ShouldProduceDifferentHashes()
    {
        // Arrange
        using TemporaryTestDirectory temporaryTestDirectory = new();
        string filePath = temporaryTestDirectory.CreateFile("book.pdf", "original content");
        List<HashedFileSystemFileDto> firstInputFiles = [_hashedFileSystemFileDtoFixture.Create(path: filePath, size: new FileInfo(filePath).Length, currentHash: 0, oldHash: 0, ticks: 0)];

        // Act
        List<HashedFileSystemFileDto> firstResult = await _sut.HashFilesAsync(firstInputFiles, () => Task.CompletedTask, CancellationToken.None);
        await File.WriteAllTextAsync(filePath, "changed content");
        List<HashedFileSystemFileDto> secondInputFiles = [_hashedFileSystemFileDtoFixture.Create(path: filePath, size: new FileInfo(filePath).Length, currentHash: 0, oldHash: 0, ticks: 0)];
        List<HashedFileSystemFileDto> secondResult = await _sut.HashFilesAsync(secondInputFiles, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        Assert.NotEqual(firstResult[0].CurrentHash, secondResult[0].CurrentHash);
    }

    [Fact]
    public async Task HashFilesAsync_WhenFileDoesNotExist_ShouldSkipFileAndNotThrow()
    {
        // Arrange
        string nonExistentFilePath = Path.Combine(Path.GetTempPath(), "does-not-exist.pdf");
        List<HashedFileSystemFileDto> inputFiles = [_hashedFileSystemFileDtoFixture.Create(path: nonExistentFilePath, size: 100, currentHash: 0, oldHash: 0, ticks: 0)];

        // Act
        List<HashedFileSystemFileDto> result = await _sut.HashFilesAsync(inputFiles, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HashFilesAsync_WhenOneFileIsMissing_ShouldHashTheExistingFilesOnly()
    {
        // Arrange
        using TemporaryTestDirectory temporaryTestDirectory = new();
        string existingFilePath = temporaryTestDirectory.CreateFile("book.pdf", "content");
        List<HashedFileSystemFileDto> inputFiles =
        [
            _hashedFileSystemFileDtoFixture.Create(path: existingFilePath, size: new FileInfo(existingFilePath).Length, currentHash: 0, oldHash: 0, ticks: 0),
            _hashedFileSystemFileDtoFixture.Create(path: Path.Combine(temporaryTestDirectory.Path, "missing.pdf"), size: 100, currentHash: 0, oldHash: 0, ticks: 0)
        ];

        // Act
        List<HashedFileSystemFileDto> result = await _sut.HashFilesAsync(inputFiles, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        HashedFileSystemFileDto hashedFile = Assert.Single(result);
        Assert.Equal(existingFilePath, hashedFile.Path);
        Assert.NotEqual(0UL, hashedFile.CurrentHash);
    }

    [Fact]
    public async Task HashFilesAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        using TemporaryTestDirectory temporaryTestDirectory = new();
        string filePath = temporaryTestDirectory.CreateFile("book.pdf", "content");
        List<HashedFileSystemFileDto> inputFiles = [_hashedFileSystemFileDtoFixture.Create(path: filePath, size: new FileInfo(filePath).Length, currentHash: 0, oldHash: 0, ticks: 0)];
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task<List<HashedFileSystemFileDto>> operation = _sut.HashFilesAsync(inputFiles, () => Task.CompletedTask, cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
    }

    /// <summary>
    /// Test helper managing a temporary directory, deleted when the test finishes.
    /// </summary>
    private sealed class TemporaryTestDirectory : IDisposable
    {
        private bool _disposed;

        public TemporaryTestDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"lumina-file-hash-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CreateFile(string fileName, string content)
        {
            string filePath = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    Directory.Delete(Path, recursive: true);
                }
                catch (IOException)
                {
                    // best effort cleanup of the temporary test directory
                }
                _disposed = true;
            }
        }
    }
}
