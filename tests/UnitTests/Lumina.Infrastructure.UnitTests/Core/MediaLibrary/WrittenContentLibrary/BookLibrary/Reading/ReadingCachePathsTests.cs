#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingCachePaths"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingCachePathsTests
{
    [Fact]
    public void GetRootDirectory_WhenCalled_ShouldReturnBaseDirectoryWithReadingCacheDirectoryName()
    {
        // Act
        string result = ReadingCachePaths.GetRootDirectory();

        // Assert
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, ReadingCachePaths.DIRECTORY_NAME), result);
    }

    [Fact]
    public void DirectoryName_WhenAccessed_ShouldReturnReadingCache()
    {
        // Assert
        Assert.Equal("reading-cache", ReadingCachePaths.DIRECTORY_NAME);
    }
}
