#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistDetailsDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistDetailsDtoMappingTests
{
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PlaylistDetailsDtoFixture _playlistDetailsDtoFixture = new();
    private readonly PaginatedResultDtoFixture<PlaylistEntryDto> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingPlaylistDetails_ShouldMapThePlaylistFieldsAndThePaginatedEntries()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        DateTime createdOnUtc = new(2024, 4, 5, 6, 7, 8, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 4, 6, 7, 8, 9, DateTimeKind.Utc);
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            name: "Evening",
            description: "Wind down",
            type: PlaylistType.Music,
            createdOnUtc: createdOnUtc,
            includeUpdatedOnUtc: true,
            updatedOnUtc: updatedOnUtc);
        PlaylistEntryDto entry = _playlistEntryDtoFixture.Create(displayPosition: 0);
        List<PlaylistEntryDto> entries = [entry];
        PaginatedResultDto<PlaylistEntryDto> entryPage = _paginatedResultDtoFixture.Create(
            data: entries,
            currentPage: 2,
            perPage: 50,
            count: 51,
            numberOfPages: 2);
        PlaylistDetailsDto details = _playlistDetailsDtoFixture.Create(playlist: playlist, entries: entryPage);

        // Act
        PlaylistResponse response = details.ToResponse();

        // Assert
        Assert.Equal(playlistId, response.Id);
        Assert.Equal("Evening", response.Name);
        Assert.Equal("Wind down", response.Description);
        Assert.Equal(PlaylistType.Music, response.Type);
        Assert.Equal(createdOnUtc, response.CreatedOnUtc);
        Assert.True(response.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, response.UpdatedOnUtc.Value);
        PlaylistEntryResponse entryResponse = Assert.Single(response.Entries.Data);
        Assert.Equal(entry.Entry.Id, entryResponse.Id);
        Assert.Equal(2, response.Entries.CurrentPage);
        Assert.Equal(50, response.Entries.PerPage);
        Assert.Equal(51, response.Entries.Count);
        Assert.Equal(2, response.Entries.NumberOfPages);
    }

    [Fact]
    public void ToResponse_WhenDescriptionAndUpdatedOnUtcAreMissing_ShouldMapNulls()
    {
        // Arrange
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            includeDescription: false,
            includeUpdatedOnUtc: false);
        PlaylistDetailsDto details = _playlistDetailsDtoFixture.Create(
            playlist: playlist,
            entries: _paginatedResultDtoFixture.Create());

        // Act
        PlaylistResponse response = details.ToResponse();

        // Assert
        Assert.Null(response.Description);
        Assert.Null(response.UpdatedOnUtc);
    }
}
