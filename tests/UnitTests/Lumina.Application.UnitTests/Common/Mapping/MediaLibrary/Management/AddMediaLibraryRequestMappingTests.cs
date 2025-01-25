#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.LibraryType.Should().Be(request.LibraryType);
        result.ContentLocations.Should().BeEquivalentTo(request.ContentLocations);
        result.CoverImage.Should().BeEquivalentTo(request.CoverImage);
        result.IsEnabled.Should().Be(request.IsEnabled);
        result.IsLocked.Should().Be(request.IsLocked);
        result.DownloadMedatadaFromWeb.Should().Be(request.DownloadMedatadaFromWeb);
        result.SaveMetadataInMediaDirectories.Should().Be(request.SaveMetadataInMediaDirectories);
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
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.LibraryType.Should().Be(libraryType.ToString());
        result.ContentLocations.Should().BeEquivalentTo(request.ContentLocations);
        result.CoverImage.Should().BeEquivalentTo(request.CoverImage);
        result.IsEnabled.Should().Be(request.IsEnabled);
        result.IsLocked.Should().Be(request.IsLocked);
        result.DownloadMedatadaFromWeb.Should().Be(request.DownloadMedatadaFromWeb);
        result.SaveMetadataInMediaDirectories.Should().Be(request.SaveMetadataInMediaDirectories);
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
        result.Should().NotBeNull();
        result.ContentLocations.Should().BeEquivalentTo(contentLocations);
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
        result.Should().NotBeNull();
        result.CoverImage.Should().BeNull();
    }
}
