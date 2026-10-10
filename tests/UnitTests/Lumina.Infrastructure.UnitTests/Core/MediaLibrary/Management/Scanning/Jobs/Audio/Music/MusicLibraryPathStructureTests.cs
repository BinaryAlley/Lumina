#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Contains unit tests for the <see cref="MusicLibraryPathStructure"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryPathStructureTests
{
    private readonly IPathService _mockPathService;
    private readonly MusicLibraryPathStructure _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicLibraryPathStructureTests"/> class.
    /// </summary>
    public MusicLibraryPathStructureTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _sut = new MusicLibraryPathStructure(_mockPathService);
    }

    [Theory]
    [InlineData('\\', @"C:\Music\Queen\A Night at the Opera", @"C:\Music\Queen\A Night at the Opera\Bohemian Rhapsody.flac")] // Windows separated path
    [InlineData('/', "/music/queen/a-night-at-the-opera", "/music/queen/a-night-at-the-opera/bohemian-rhapsody.flac")] // Unix separated path
    public void GetAlbumDirectory_WhenTheTrackIsInTheAlbumDirectory_ShouldReturnTheTrackDirectory(char separator, string albumDirectory, string trackPath)
    {
        // Arrange
        _mockPathService.PathSeparator.Returns(separator);

        // Act
        string result = _sut.GetAlbumDirectory(trackPath);

        // Assert
        Assert.Equal(albumDirectory, result);
    }

    [Theory]
    [InlineData('\\', "Disc 1")] // canonical disc directory name, Windows separated path
    [InlineData('\\', "disc 2")] // lowercase disc directory name, Windows separated path
    [InlineData('\\', "CD3")] // compact disc directory name, Windows separated path
    [InlineData('\\', "disk4")] // disk directory name, Windows separated path
    [InlineData('/', "Disc 1")] // canonical disc directory name, Unix separated path
    [InlineData('/', "disc 2")] // lowercase disc directory name, Unix separated path
    [InlineData('/', "CD3")] // compact disc directory name, Unix separated path
    [InlineData('/', "disk4")] // disk directory name, Unix separated path
    public void GetAlbumDirectory_WhenTheTrackIsInADiscDirectory_ShouldReturnTheParentAlbumDirectory(char separator, string discDirectoryName)
    {
        // Arrange
        _mockPathService.PathSeparator.Returns(separator);
        string albumDirectory = string.Join(separator, "music", "queen", "a-night-at-the-opera");
        string trackPath = string.Join(separator, albumDirectory, discDirectoryName, "bohemian-rhapsody.flac");

        // Act
        string result = _sut.GetAlbumDirectory(trackPath);

        // Assert
        Assert.Equal(albumDirectory, result);
    }

    [Theory]
    [InlineData('\\')] // Windows separated path
    [InlineData('/')] // Unix separated path
    public void GetAlbumDirectory_WhenTheDirectoryIsNotADiscDirectory_ShouldReturnTheTrackDirectory(char separator)
    {
        // Arrange
        _mockPathService.PathSeparator.Returns(separator);
        string albumDirectory = string.Join(separator, "music", "queen", "live");
        string trackPath = string.Join(separator, albumDirectory, "bohemian-rhapsody.flac");

        // Act
        string result = _sut.GetAlbumDirectory(trackPath);

        // Assert
        Assert.Equal(albumDirectory, result);
    }

    [Fact]
    public void GetAlbumDirectory_WhenThePathIsNullOrEmpty_ShouldReturnTheProvidedPath()
    {
        // Act
        string nullResult = _sut.GetAlbumDirectory(null!);
        string emptyResult = _sut.GetAlbumDirectory(string.Empty);

        // Assert
        Assert.Null(nullResult);
        Assert.Equal(string.Empty, emptyResult);
    }

    [Theory]
    [InlineData('\\')] // Windows separated path
    [InlineData('/')] // Unix separated path
    public void GetAlbumDirectory_WhenThePathHasNoDirectory_ShouldReturnTheProvidedPath(char separator)
    {
        // Arrange
        _mockPathService.PathSeparator.Returns(separator);
        const string FILE_NAME_ONLY = "Bohemian Rhapsody.flac";

        // Act
        string result = _sut.GetAlbumDirectory(FILE_NAME_ONLY);

        // Assert
        Assert.Equal(FILE_NAME_ONLY, result);
    }

    [Theory]
    [InlineData('\\', "Album")] // canonical release type directory, Windows separated path
    [InlineData('\\', "album")] // lowercase release type directory, Windows separated path
    [InlineData('/', "Album")] // canonical release type directory, Unix separated path
    [InlineData('/', "album")] // lowercase release type directory, Unix separated path
    public void GetReleaseTypeDirectoryName_WhenTheParentOfTheAlbumDirectoryIsAReleaseType_ShouldReturnIt(char separator, string releaseTypeDirectoryName)
    {
        // Arrange
        _mockPathService.PathSeparator.Returns(separator);
        string trackPath = string.Join(separator, "music", "queen", releaseTypeDirectoryName, "a-night-at-the-opera", "bohemian-rhapsody.flac");

        // Act
        string? result = _sut.GetReleaseTypeDirectoryName(trackPath);

        // Assert
        Assert.Equal(releaseTypeDirectoryName, result);
    }

    [Theory]
    [InlineData('\\')] // Windows separated path
    [InlineData('/')] // Unix separated path
    public void GetReleaseTypeDirectoryName_WhenTheAlbumIsMultiDisc_ShouldReturnTheReleaseTypeDirectory(char separator)
    {
        // Arrange
        _mockPathService.PathSeparator.Returns(separator);
        string trackPath = string.Join(separator, "music", "queen", "Album", "a-night-at-the-opera", "Disc 1", "bohemian-rhapsody.flac");

        // Act
        string? result = _sut.GetReleaseTypeDirectoryName(trackPath);

        // Assert
        Assert.Equal("Album", result);
    }

    [Theory]
    [InlineData('\\')] // Windows separated path
    [InlineData('/')] // Unix separated path
    public void GetReleaseTypeDirectoryName_WhenTheParentOfTheAlbumDirectoryIsNotAReleaseType_ShouldReturnNull(char separator)
    {
        // Arrange
        _mockPathService.PathSeparator.Returns(separator);
        string trackPath = string.Join(separator, "music", "queen", "a-night-at-the-opera", "bohemian-rhapsody.flac");

        // Act
        string? result = _sut.GetReleaseTypeDirectoryName(trackPath);

        // Assert
        Assert.Null(result);
    }
}
