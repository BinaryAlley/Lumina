#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="UpdateLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryRequestTests
{
    private readonly UpdateLibraryRequestFixture _updateLibraryRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingUpdateLibraryRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        UpdateLibraryRequest expected = _updateLibraryRequestFixture.Create(
            id: id,
            userId: userId,
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
        UpdateLibraryRequest? actual = JsonSerializer.Deserialize<UpdateLibraryRequest>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        UpdateLibraryRequest sut = _updateLibraryRequestFixture.Create(
            id: id,
            userId: userId,
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
        (Guid sutId, Guid sutUserId, string? title, string? libraryType, string[]? contentLocations, string? coverImage, bool isEnabled, bool isLocked, bool canDownloadMetadataFromWeb, bool shouldSaveMetadataInMediaDirectories, bool shouldSkipUnchangedDirectoriesDuringScan) = sut;

        // Assert
        Assert.Equal(sut.Id, sutId);
        Assert.Equal(sut.UserId, sutUserId);
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
