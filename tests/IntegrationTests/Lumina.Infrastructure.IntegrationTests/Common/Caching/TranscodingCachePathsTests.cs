#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Caching;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Common.Caching;

/// <summary>
/// Contains integration tests for the <see cref="TranscodingCachePaths"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingCachePathsTests : IDisposable
{
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCachePathsTests"/> class.
    /// </summary>
    public TranscodingCachePathsTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-transcoding-cache-paths-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Theory]
    [InlineData(null)] // no cache directory is configured
    [InlineData("")] // an empty cache directory is configured
    [InlineData("   ")] // a white space cache directory is configured
    public void GetRootDirectory_WhenNoDirectoryIsConfigured_ShouldReturnTheDefaultDirectoryUnderTheTemporaryDirectory(string? configuredDirectory)
    {
        // Act
        string result = TranscodingCachePaths.GetRootDirectory(configuredDirectory);

        // Assert
        string directoryName = Path.GetFileName(result);
        Assert.Equal(Path.Combine(Path.GetTempPath(), directoryName), result);
        Assert.StartsWith(TranscodingCachePaths.DEFAULT_DIRECTORY_NAME + "-", directoryName, StringComparison.Ordinal);
        string accountSuffix = directoryName[(TranscodingCachePaths.DEFAULT_DIRECTORY_NAME.Length + 1)..];
        Assert.Matches("^[0-9a-f]{16}$", accountSuffix);
    }

    [Fact]
    public void GetRootDirectory_WhenCalledRepeatedly_ShouldReturnTheSamePath()
    {
        // Act
        string firstResult = TranscodingCachePaths.GetRootDirectory(null);
        string secondResult = TranscodingCachePaths.GetRootDirectory(null);

        // Assert
        Assert.Equal(firstResult, secondResult);
    }

    [Fact]
    public void GetRootDirectory_WhenTheConfiguredDirectoryIsRooted_ShouldReturnItsFullPath()
    {
        // Arrange
        string configuredDirectory = Path.Combine(_temporaryDirectory, "configured-cache");

        // Act
        string result = TranscodingCachePaths.GetRootDirectory($"  {configuredDirectory}  ");

        // Assert
        Assert.Equal(Path.GetFullPath(configuredDirectory), result);
    }

    [Fact]
    public void GetRootDirectory_WhenTheConfiguredDirectoryIsRelative_ShouldResolveItAgainstTheBaseDirectory()
    {
        // Arrange
        string configuredDirectory = Path.Combine("relative", "cache");

        // Act
        string result = TranscodingCachePaths.GetRootDirectory(configuredDirectory);

        // Assert
        Assert.Equal(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredDirectory)), result);
    }

    [Fact]
    public void HasCacheMarker_WhenTheDirectoryHasNoMarker_ShouldReturnFalse()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "unmarked");
        Directory.CreateDirectory(directoryPath);

        // Act
        bool result = TranscodingCachePaths.HasCacheMarker(directoryPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WriteCacheMarker_WhenTheDirectoryHasNoMarker_ShouldWriteTheDocumentedMarkerFile()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "marked");
        Directory.CreateDirectory(directoryPath);

        // Act
        TranscodingCachePaths.WriteCacheMarker(directoryPath);

        // Assert
        Assert.Equal(".lumina-transcoding-cache", TranscodingCachePaths.CACHE_MARKER_FILE_NAME);
        Assert.True(TranscodingCachePaths.HasCacheMarker(directoryPath));
        Assert.True(File.Exists(Path.Combine(directoryPath, TranscodingCachePaths.CACHE_MARKER_FILE_NAME)));
    }

    [Fact]
    public void WriteCacheMarker_WhenCalledTwice_ShouldNotRewriteTheMarker()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "already-marked");
        Directory.CreateDirectory(directoryPath);
        TranscodingCachePaths.WriteCacheMarker(directoryPath);
        string markerPath = Path.Combine(directoryPath, TranscodingCachePaths.CACHE_MARKER_FILE_NAME);
        File.WriteAllText(markerPath, "operator content");

        // Act
        TranscodingCachePaths.WriteCacheMarker(directoryPath);

        // Assert
        Assert.Equal("operator content", File.ReadAllText(markerPath));
    }

    [Theory]
    [InlineData(null)] // no identifier
    [InlineData("")] // an empty identifier
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcde")] // 63 characters
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0")] // 65 characters
    [InlineData("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF")] // uppercase hexadecimal characters
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdeg")] // a character that is not hexadecimal
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcde/")] // a directory separator
    public void IsValidCacheId_WhenTheValueIsNotAWellFormedIdentifier_ShouldReturnFalse(string? cacheId)
    {
        // Act
        bool result = TranscodingCachePaths.IsValidCacheId(cacheId!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidCacheId_WhenTheValueIsA64CharacterLowercaseHexIdentifier_ShouldReturnTrue()
    {
        // Arrange
        string cacheId = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

        // Act
        bool result = TranscodingCachePaths.IsValidCacheId(cacheId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)] // no name
    [InlineData("")] // an empty name
    [InlineData("segment.m4s")] // an ordinary file name
    [InlineData("not-a-cache-entry")] // an ordinary directory name
    public void IsCacheEntryName_WhenTheNameIsNotInTheCacheNamespace_ShouldReturnFalse(string? entryName)
    {
        // Act
        bool result = TranscodingCachePaths.IsCacheEntryName(entryName!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")] // a cache identifier
    [InlineData("tmp-0123456789abcdef")] // a temporary directory
    [InlineData("del-0123456789abcdef")] // a deletion tombstone
    public void IsCacheEntryName_WhenTheNameIsInTheCacheNamespace_ShouldReturnTrue(string entryName)
    {
        // Act
        bool result = TranscodingCachePaths.IsCacheEntryName(entryName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ClearCacheEntries_WhenCalled_ShouldRemoveOnlyCacheEntriesAndKeepTheRoot()
    {
        // Arrange
        string cacheRoot = Path.Combine(_temporaryDirectory, "clear-cache");
        Directory.CreateDirectory(cacheRoot);
        string cacheEntryPath = Path.Combine(cacheRoot, CreateCacheId());
        Directory.CreateDirectory(Path.Combine(cacheEntryPath, "nested"));
        File.WriteAllText(Path.Combine(cacheEntryPath, "nested", "segment.m4s"), "data");
        string temporaryEntryPath = Path.Combine(cacheRoot, "tmp-0123456789abcdef");
        Directory.CreateDirectory(temporaryEntryPath);
        string deletionEntryPath = Path.Combine(cacheRoot, "del-0123456789abcdef");
        Directory.CreateDirectory(deletionEntryPath);
        string unrelatedDirectoryPath = Path.Combine(cacheRoot, "unrelated");
        Directory.CreateDirectory(unrelatedDirectoryPath);
        File.WriteAllText(Path.Combine(unrelatedDirectoryPath, "keep.txt"), "keep");
        string unrelatedFilePath = Path.Combine(cacheRoot, "unrelated.txt");
        File.WriteAllText(unrelatedFilePath, "keep");

        // Act
        TranscodingCachePaths.ClearCacheEntries(cacheRoot, CancellationToken.None);

        // Assert
        Assert.True(Directory.Exists(cacheRoot));
        Assert.False(Directory.Exists(cacheEntryPath));
        Assert.False(Directory.Exists(temporaryEntryPath));
        Assert.False(Directory.Exists(deletionEntryPath));
        Assert.True(Directory.Exists(unrelatedDirectoryPath));
        Assert.True(File.Exists(Path.Combine(unrelatedDirectoryPath, "keep.txt")));
        Assert.True(File.Exists(unrelatedFilePath));
    }

    [Fact]
    public void ClearCacheEntries_WhenACacheEntryIsASymbolicLink_ShouldDeleteTheLinkAndKeepItsTarget()
    {
        // Arrange
        string targetPath = Path.Combine(_temporaryDirectory, "link-target");
        Directory.CreateDirectory(targetPath);
        File.WriteAllText(Path.Combine(targetPath, "keep.txt"), "keep");
        string cacheRoot = Path.Combine(_temporaryDirectory, "link-cache");
        Directory.CreateDirectory(cacheRoot);
        string linkPath = Path.Combine(cacheRoot, CreateCacheId());
        if (!TryCreateJunction(linkPath, targetPath))
        {
            // The environment does not support creating reparse points.
            return;
        }

        Assert.NotNull(new DirectoryInfo(linkPath).LinkTarget);

        // Act
        TranscodingCachePaths.ClearCacheEntries(cacheRoot, CancellationToken.None);

        // Assert
        Assert.False(Directory.Exists(linkPath));
        Assert.True(Directory.Exists(targetPath));
        Assert.True(File.Exists(Path.Combine(targetPath, "keep.txt")));
    }

    [Fact]
    public void EnsureDirectoryExists_WhenTheDirectoryDoesNotExist_ShouldCreateItAndMarkIt()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "created-cache");

        // Act
        TranscodingCachePaths.EnsureDirectoryExists(directoryPath);

        // Assert
        Assert.True(Directory.Exists(directoryPath));
        Assert.True(TranscodingCachePaths.HasCacheMarker(directoryPath));
    }

    [Fact]
    public void EnsureDirectoryExists_WhenTheDirectoryExistsWithoutAMarker_ShouldNotMarkIt()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "existing-cache");
        Directory.CreateDirectory(directoryPath);

        // Act
        TranscodingCachePaths.EnsureDirectoryExists(directoryPath);

        // Assert
        Assert.True(Directory.Exists(directoryPath));
        Assert.False(TranscodingCachePaths.HasCacheMarker(directoryPath));
    }

    [Fact]
    public void TryDeleteDirectory_WhenTheDirectoryExists_ShouldDeleteItAndReturnTrue()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "deletable-cache");
        Directory.CreateDirectory(Path.Combine(directoryPath, "nested"));
        File.WriteAllBytes(Path.Combine(directoryPath, "nested", "segment.m4s"), new byte[8]);

        // Act
        bool result = TranscodingCachePaths.TryDeleteDirectory(directoryPath);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(directoryPath));
    }

    [Fact]
    public void TryDeleteDirectory_WhenTheDirectoryDoesNotExist_ShouldReturnTrue()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "absent-cache");

        // Act
        bool result = TranscodingCachePaths.TryDeleteDirectory(directoryPath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryDeleteDirectory_WhenTheDirectoryContainsAnOpenFile_ShouldReportWhetherThePlatformDeletedIt()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "open-file-cache");
        Directory.CreateDirectory(directoryPath);
        string openFilePath = Path.Combine(directoryPath, "open.bin");
        File.WriteAllBytes(openFilePath, new byte[8]);

        // Act
        bool result;
        using (FileStream openStream = new(openFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
            result = TranscodingCachePaths.TryDeleteDirectory(directoryPath);

        // Assert
        if (OperatingSystem.IsWindows())
        {
            // Windows refuses to delete a file that another handle holds without delete sharing.
            Assert.False(result);
            Assert.True(Directory.Exists(directoryPath));
            Assert.True(File.Exists(openFilePath));
        }
        else
        {
            // Unix-like platforms allow deleting a file that is still open.
            Assert.True(result);
            Assert.False(Directory.Exists(directoryPath));
        }
    }

    [Fact]
    public void RestrictPermissions_WhenTheDirectoryIsASymbolicLink_ShouldThrowIOException()
    {
        // Arrange
        string targetPath = Path.Combine(_temporaryDirectory, "permissions-target");
        Directory.CreateDirectory(targetPath);
        string linkPath = Path.Combine(_temporaryDirectory, "permissions-link");
        if (!TryCreateJunction(linkPath, targetPath))
        {
            // The environment does not support creating reparse points.
            return;
        }

        Assert.NotNull(new DirectoryInfo(linkPath).LinkTarget);

        // Act
        IOException exception = Assert.Throws<IOException>(() => TranscodingCachePaths.RestrictPermissions(linkPath));

        // Assert
        Assert.Contains("symbolic link", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestrictPermissions_WhenTheDirectoryIsNotASymbolicLink_ShouldNotThrow()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "plain-cache");
        Directory.CreateDirectory(directoryPath);

        // Act
        Exception? exception = Record.Exception(() => TranscodingCachePaths.RestrictPermissions(directoryPath));

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Creates a well formed opaque cache identifier.
    /// </summary>
    /// <returns>A 64 character lowercase hexadecimal identifier.</returns>
    private static string CreateCacheId()
    {
        return $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Creates a file system link that points to the target directory: a junction on Windows, and a symbolic link on Unix-like platforms.
    /// </summary>
    /// <param name="linkPath">The file system path of the link to create.</param>
    /// <param name="targetDirectory">The file system path of the directory the link points to.</param>
    /// <returns><see langword="true"/> when the link was created, <see langword="false"/> otherwise.</returns>
    private static bool TryCreateJunction(string linkPath, string targetDirectory)
    {
        try
        {
            ProcessStartInfo startInfo = OperatingSystem.IsWindows()
                ? new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c mklink /J \"{linkPath}\" \"{targetDirectory}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
                : new()
                {
                    FileName = "ln",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
            if (!OperatingSystem.IsWindows())
            {
                startInfo.ArgumentList.Add("-s");
                startInfo.ArgumentList.Add(targetDirectory);
                startInfo.ArgumentList.Add(linkPath);
            }

            using (Process process = Process.Start(startInfo)!)
            {
                process.WaitForExit(5000);
                return process.ExitCode == 0;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_temporaryDirectory))
                Directory.Delete(_temporaryDirectory, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // A symbolic link may keep a target alive on some platforms; the directory is under the process temporary directory.
        }
        GC.SuppressFinalize(this);
    }
}
