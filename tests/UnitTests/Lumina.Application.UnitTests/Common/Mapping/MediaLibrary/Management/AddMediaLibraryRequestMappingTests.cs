#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="AddMediaLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddMediaLibraryRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        AddLibraryRequest request = new(
            "My Library",
            "Book",
            ["C:/Books", "D:/Media/Books"],
            "D:/poster.jpg",
            true,
            false,
            true,
            false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.LibraryType, result.LibraryType);
        Assert.Equal(request.ContentLocations, result.ContentLocations);
        Assert.Equal(request.CoverImage, result.CoverImage);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
        Assert.Equal(request.IsLocked, result.IsLocked);
        Assert.Equal(request.DownloadMedatadaFromWeb, result.DownloadMedatadaFromWeb);
        Assert.Equal(request.SaveMetadataInMediaDirectories, result.SaveMetadataInMediaDirectories);
    }

    [Theory]
    [InlineData(LibraryType.Book)]
    [InlineData(LibraryType.Movie)]
    [InlineData(LibraryType.TvShow)]
    [InlineData(LibraryType.Music)]
    public void ToCommand_WhenMappingDifferentLibraryTypes_ShouldMapCorrectly(LibraryType libraryType)
    {
        // Arrange
        AddLibraryRequest request = new(
            "My Library",
            libraryType.ToString(),
            ["C:/Media"],
            "D:/poster.jpg",
            true,
            false,
            true,
            false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(libraryType.ToString(), result.LibraryType);
        Assert.Equal(request.ContentLocations, result.ContentLocations);
        Assert.Equal(request.CoverImage, result.CoverImage);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
        Assert.Equal(request.IsLocked, result.IsLocked);
        Assert.Equal(request.DownloadMedatadaFromWeb, result.DownloadMedatadaFromWeb);
        Assert.Equal(request.SaveMetadataInMediaDirectories, result.SaveMetadataInMediaDirectories);
    }

    [Fact]
    public void ToCommand_WhenMappingMultipleContentLocations_ShouldMapAllCorrectly()
    {
        // Arrange
        string[] contentLocations =
        [
            "C:/Media/Books",
            "D:/Books",
            "E:/Digital Library/Books",
            "F:/Reading Material"
        ];

        AddLibraryRequest request = new(
            "My Library",
            "Book",
            contentLocations,
            "D:/poster.jpg",
            true,
            false,
            true,
            false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contentLocations, result.ContentLocations);
    }

    [Fact]
    public void ToCommand_WhenMappingWithNullCover_ShouldMapCorrectly()
    {
        // Arrange
        string[] contentLocations =
        [
            "C:/Media/Books",
            "D:/Books"
        ];

        AddLibraryRequest request = new(
            "My Library",
            "Book",
            contentLocations,
            null,
            true,
            false,
            true,
            false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CoverImage);
    }
}
