#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="LibraryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryResponseTests
{
    private readonly LibraryResponseFixture _libraryResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingLibraryResponse_ShouldPreserveValues()
    {
        // Arrange
        LibraryResponse expected = _libraryResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LibraryResponse? actual = JsonSerializer.Deserialize<LibraryResponse>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingLibraryResponse_ShouldSerializeLibraryTypeAsCamelCaseString()
    {
        // Arrange
        LibraryResponse sut = _libraryResponseFixture.Create(libraryType: LibraryType.Movie);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"LibraryType\":\"movie\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        LibraryResponse sut = _libraryResponseFixture.Create();

        // Act
        (Guid id, Guid userId, string title, LibraryType libraryType, List<string> contentLocations, string? coverImage, bool isEnabled, bool isLocked, bool downloadMetadataFromWeb, bool shouldSaveMetadataInMediaDirectories, bool shouldSkipUnchangedDirectoriesDuringScan, DateTime createdOnUtc, DateTime? updatedOnUtc) = sut;

        // Assert
        Assert.Equal(sut.Id, id);
        Assert.Equal(sut.UserId, userId);
        Assert.Equal(sut.Title, title);
        Assert.Equal(sut.LibraryType, libraryType);
        Assert.Equal(sut.ContentLocations, contentLocations);
        Assert.Equal(sut.CoverImage, coverImage);
        Assert.Equal(sut.IsEnabled, isEnabled);
        Assert.Equal(sut.IsLocked, isLocked);
        Assert.Equal(sut.DownloadMetadataFromWeb, downloadMetadataFromWeb);
        Assert.Equal(sut.ShouldSaveMetadataInMediaDirectories, shouldSaveMetadataInMediaDirectories);
        Assert.Equal(sut.ShouldSkipUnchangedDirectoriesDuringScan, shouldSkipUnchangedDirectoriesDuringScan);
        Assert.Equal(sut.CreatedOnUtc, createdOnUtc);
        Assert.Equal(sut.UpdatedOnUtc, updatedOnUtc);
    }
}
