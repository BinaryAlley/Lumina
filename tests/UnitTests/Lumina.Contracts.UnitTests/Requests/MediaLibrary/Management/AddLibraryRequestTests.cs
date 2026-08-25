#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="AddLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryRequestTests
{
    private readonly AddLibraryRequestFixture _addLibraryRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingAddLibraryRequest_ShouldPreserveValues()
    {
        // Arrange
        AddLibraryRequest expected = _addLibraryRequestFixture.Create(
            title: "Books",
            libraryType: "Book",
            contentLocations: [@"C:\Media\Books"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        AddLibraryRequest? actual = JsonSerializer.Deserialize<AddLibraryRequest>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        AddLibraryRequest sut = _addLibraryRequestFixture.Create(
            title: "Books",
            libraryType: "Book",
            contentLocations: [@"C:\Media\Books"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        (string? title, string? libraryType, string[]? contentLocations, string? coverImage, bool isEnabled, bool isLocked, bool canDownloadMetadataFromWeb, bool shouldSaveMetadataInMediaDirectories, bool shouldSkipUnchangedDirectoriesDuringScan) = sut;

        // Assert
        Assert.Equal(sut.Title, title);
        Assert.Equal(sut.LibraryType, libraryType);
        Assert.Equal(sut.ContentLocations, contentLocations);
        Assert.Equal(sut.CoverImage, coverImage);
        Assert.Equal(sut.IsEnabled, isEnabled);
        Assert.Equal(sut.IsLocked, isLocked);
        Assert.Equal(sut.CanDownloadMetadataFromWeb, canDownloadMetadataFromWeb);
        Assert.Equal(sut.ShouldSaveMetadataInMediaDirectories, shouldSaveMetadataInMediaDirectories);
        Assert.Equal(sut.ShouldSkipUnchangedDirectoriesDuringScan, shouldSkipUnchangedDirectoriesDuringScan);
    }
}
