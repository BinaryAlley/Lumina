#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="UpdateLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryRequestMappingTests
{
    private readonly UpdateLibraryRequestFixture _updateLibraryRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: ["C:/Books", "D:/Media/Books"],
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Equal(request.Id, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.LibraryType, result.LibraryType);
        Assert.Equal(request.ContentLocations, result.ContentLocations);
        Assert.Equal(request.CoverImage, result.CoverImage);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
        Assert.Equal(request.IsLocked, result.IsLocked);
        Assert.Equal(request.CanDownloadMetadataFromWeb, result.CanDownloadMetadataFromWeb);
        Assert.Equal(request.ShouldSaveMetadataInMediaDirectories, result.ShouldSaveMetadataInMediaDirectories);
        Assert.Equal(request.ShouldSkipUnchangedDirectoriesDuringScan, result.ShouldSkipUnchangedDirectoriesDuringScan);
    }
}
