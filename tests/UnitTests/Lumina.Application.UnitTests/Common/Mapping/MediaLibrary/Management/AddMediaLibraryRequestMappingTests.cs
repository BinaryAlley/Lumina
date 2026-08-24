#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="AddMediaLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddMediaLibraryRequestMappingTests
{
    private readonly AddLibraryRequestFixture _addLibraryRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: ["C:/Books", "D:/Media/Books"],
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false
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
        Assert.Equal(request.CanDownloadMetadataFromWeb, result.CanDownloadMetadataFromWeb);
        Assert.Equal(request.ShouldSaveMetadataInMediaDirectories, result.ShouldSaveMetadataInMediaDirectories);
    }

    [Theory]
    [InlineData(LibraryType.Book)]
    [InlineData(LibraryType.Movie)]
    [InlineData(LibraryType.TvShow)]
    [InlineData(LibraryType.Music)]
    public void ToCommand_WhenMappingDifferentLibraryTypes_ShouldMapCorrectly(LibraryType libraryType)
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: libraryType.ToString(),
            contentLocations: ["C:/Media"],
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false
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
        Assert.Equal(request.CanDownloadMetadataFromWeb, result.CanDownloadMetadataFromWeb);
        Assert.Equal(request.ShouldSaveMetadataInMediaDirectories, result.ShouldSaveMetadataInMediaDirectories);
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

        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: contentLocations,
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false
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

        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: contentLocations,
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CoverImage);
    }
}
