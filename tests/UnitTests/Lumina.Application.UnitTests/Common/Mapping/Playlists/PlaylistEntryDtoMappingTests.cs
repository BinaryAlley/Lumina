#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistEntryDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryDtoMappingTests
{
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PlaylistEntryDisplayDtoFixture _playlistEntryDisplayDtoFixture = new();
    private readonly PaginatedResultDtoFixture<PlaylistEntryDto> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenDisplayIsMissing_ShouldReturnTheStoredEntryWithEmptyDisplayFields()
    {
        // Arrange
        PlaylistEntryEntity entity = _playlistEntryEntityFixture.Create();
        PlaylistEntryDto entry = _playlistEntryDtoFixture.Create(entry: entity, displayPosition: 3, includeDisplay: false);

        // Act
        PlaylistEntryResponse response = entry.ToResponse();

        // Assert
        Assert.Equal(entity.Id, response.Id);
        Assert.Equal(entity.MediaItemId, response.MediaItemId);
        Assert.Equal(entity.MediaItemType, response.MediaItemType);
        Assert.Equal(3, response.Position);
        Assert.Equal(Guid.Empty, response.LibraryId);
        Assert.Equal(Guid.Empty, response.CollectionId);
        Assert.Equal(Guid.Empty, response.CreatorId);
        Assert.Equal(string.Empty, response.MediaItemTitle);
        Assert.Equal(string.Empty, response.CollectionTitle);
        Assert.Equal(string.Empty, response.CreatorName);
        Assert.Equal(0, response.DurationInSeconds);
        Assert.Null(response.CoverPath);
    }

    [Fact]
    public void ToResponse_WhenDisplayIsPresent_ShouldMapEveryDisplayField()
    {
        // Arrange
        PlaylistEntryEntity entity = _playlistEntryEntityFixture.Create();
        PlaylistEntryDisplayDto display = _playlistEntryDisplayDtoFixture.Create();
        PlaylistEntryDto entry = _playlistEntryDtoFixture.Create(entry: entity, display: display, displayPosition: 6);

        // Act
        PlaylistEntryResponse response = entry.ToResponse();

        // Assert
        Assert.Equal(entity.Id, response.Id);
        Assert.Equal(entity.MediaItemId, response.MediaItemId);
        Assert.Equal(entity.MediaItemType, response.MediaItemType);
        Assert.Equal(6, response.Position);
        Assert.Equal(display.LibraryId, response.LibraryId);
        Assert.Equal(display.AlbumId, response.CollectionId);
        Assert.Equal(display.ArtistId, response.CreatorId);
        Assert.Equal(display.TrackTitle, response.MediaItemTitle);
        Assert.Equal(display.AlbumTitle, response.CollectionTitle);
        Assert.Equal(display.ArtistName, response.CreatorName);
        Assert.Equal(display.DurationInSeconds, response.DurationInSeconds);
        Assert.Equal(display.CoverPath, response.CoverPath);
    }

    [Fact]
    public void ToResponse_WhenDisplayHasNoCoverPath_ShouldMapNullCoverPath()
    {
        // Arrange
        PlaylistEntryDisplayDto display = _playlistEntryDisplayDtoFixture.Create(includeCoverPath: false);
        PlaylistEntryDto entry = _playlistEntryDtoFixture.Create(display: display);

        // Act
        PlaylistEntryResponse response = entry.ToResponse();

        // Assert
        Assert.Null(response.CoverPath);
    }

    [Fact]
    public void ToResponses_WhenMappingAPage_ShouldPreserveTheOrderOfTheEntriesAndThePaginationMetadata()
    {
        // Arrange
        PlaylistEntryDto firstEntry = _playlistEntryDtoFixture.Create(displayPosition: 0);
        PlaylistEntryDto secondEntry = _playlistEntryDtoFixture.Create(displayPosition: 1);
        PlaylistEntryDto thirdEntry = _playlistEntryDtoFixture.Create(displayPosition: 2, includeDisplay: false);
        List<PlaylistEntryDto> entries = [firstEntry, secondEntry, thirdEntry];
        PaginatedResultDto<PlaylistEntryDto> page = _paginatedResultDtoFixture.Create(
            data: entries,
            currentPage: 2,
            perPage: 3,
            count: 7,
            numberOfPages: 3);

        // Act
        PaginatedResponse<PlaylistEntryResponse> response = page.ToResponses();

        // Assert
        Assert.Equal(3, response.Data.Count);
        Assert.Equal<Guid>(
            entries.Select(entry => entry.Entry.Id),
            response.Data.Select(entry => entry.Id));
        Assert.Equal<int>([0, 1, 2], response.Data.Select(entry => entry.Position));
        Assert.Equal(2, response.CurrentPage);
        Assert.Equal(3, response.PerPage);
        Assert.Equal(7, response.Count);
        Assert.Equal(3, response.NumberOfPages);
    }
}
