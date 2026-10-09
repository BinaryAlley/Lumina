#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Contains unit tests for the <see cref="MusicLibraryPathStructure"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryPathStructureTests
{
    private const string ALBUM_DIRECTORY = @"C:\Music\Queen\A Night at the Opera";
    private const string TRACK_PATH = @"C:\Music\Queen\A Night at the Opera\Bohemian Rhapsody.flac";

    [Fact]
    public void GetAlbumDirectory_WhenTheTrackIsInTheAlbumDirectory_ShouldReturnTheTrackDirectory()
    {
        // Act
        string result = MusicLibraryPathStructure.GetAlbumDirectory(TRACK_PATH);

        // Assert
        Assert.Equal(ALBUM_DIRECTORY, result);
    }

    [Theory]
    [InlineData("Disc 1")] // canonical disc directory name
    [InlineData("disc 2")] // lowercase disc directory name
    [InlineData("CD3")] // compact disc directory name
    [InlineData("disk4")] // disk directory name
    public void GetAlbumDirectory_WhenTheTrackIsInADiscDirectory_ShouldReturnTheParentAlbumDirectory(string discDirectoryName)
    {
        // Arrange
        string trackPath = $@"{ALBUM_DIRECTORY}\{discDirectoryName}\Bohemian Rhapsody.flac";

        // Act
        string result = MusicLibraryPathStructure.GetAlbumDirectory(trackPath);

        // Assert
        Assert.Equal(ALBUM_DIRECTORY, result);
    }

    [Fact]
    public void GetAlbumDirectory_WhenTheDirectoryIsNotADiscDirectory_ShouldReturnTheTrackDirectory()
    {
        // Arrange
        const string NON_DISC_TRACK_PATH = @"C:\Music\Queen\Live\Bohemian Rhapsody.flac";

        // Act
        string result = MusicLibraryPathStructure.GetAlbumDirectory(NON_DISC_TRACK_PATH);

        // Assert
        Assert.Equal(@"C:\Music\Queen\Live", result);
    }

    [Fact]
    public void GetAlbumDirectory_WhenThePathIsNullOrEmpty_ShouldReturnTheProvidedPath()
    {
        // Act
        string nullResult = MusicLibraryPathStructure.GetAlbumDirectory(null!);
        string emptyResult = MusicLibraryPathStructure.GetAlbumDirectory(string.Empty);

        // Assert
        Assert.Null(nullResult);
        Assert.Equal(string.Empty, emptyResult);
    }

    [Fact]
    public void GetAlbumDirectory_WhenThePathHasNoDirectory_ShouldReturnTheProvidedPath()
    {
        // Arrange
        const string FILE_NAME_ONLY = "Bohemian Rhapsody.flac";

        // Act
        string result = MusicLibraryPathStructure.GetAlbumDirectory(FILE_NAME_ONLY);

        // Assert
        Assert.Equal(FILE_NAME_ONLY, result);
    }

    [Fact]
    public void GetReleaseTypeDirectoryName_WhenTheParentOfTheAlbumDirectoryIsAReleaseType_ShouldReturnIt()
    {
        // Arrange
        const string RELEASE_TYPE_TRACK_PATH = @"C:\Music\Queen\Album\A Night at the Opera\Bohemian Rhapsody.flac";

        // Act
        string? result = MusicLibraryPathStructure.GetReleaseTypeDirectoryName(RELEASE_TYPE_TRACK_PATH);

        // Assert
        Assert.Equal("Album", result);
    }

    [Fact]
    public void GetReleaseTypeDirectoryName_WhenTheReleaseTypeDirectoryIsLowercase_ShouldReturnIt()
    {
        // Arrange
        const string LOWERCASE_RELEASE_TYPE_TRACK_PATH = @"C:\Music\Queen\album\A Night at the Opera\Bohemian Rhapsody.flac";

        // Act
        string? result = MusicLibraryPathStructure.GetReleaseTypeDirectoryName(LOWERCASE_RELEASE_TYPE_TRACK_PATH);

        // Assert
        Assert.Equal("album", result);
    }

    [Fact]
    public void GetReleaseTypeDirectoryName_WhenTheAlbumIsMultiDisc_ShouldReturnTheReleaseTypeDirectory()
    {
        // Arrange
        const string MULTI_DISC_TRACK_PATH = @"C:\Music\Queen\Album\A Night at the Opera\Disc 1\Bohemian Rhapsody.flac";

        // Act
        string? result = MusicLibraryPathStructure.GetReleaseTypeDirectoryName(MULTI_DISC_TRACK_PATH);

        // Assert
        Assert.Equal("Album", result);
    }

    [Fact]
    public void GetReleaseTypeDirectoryName_WhenTheParentOfTheAlbumDirectoryIsNotAReleaseType_ShouldReturnNull()
    {
        // Arrange
        const string NON_RELEASE_TYPE_TRACK_PATH = @"C:\Music\Queen\A Night at the Opera\Bohemian Rhapsody.flac";

        // Act
        string? result = MusicLibraryPathStructure.GetReleaseTypeDirectoryName(NON_RELEASE_TYPE_TRACK_PATH);

        // Assert
        Assert.Null(result);
    }
}
