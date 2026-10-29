#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistDetailsDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistDetailsDtoFixture
{
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PaginatedResultDtoFixture<PlaylistEntryDto> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistDetailsDto"/>.
    /// </summary>
    /// <param name="playlist">Optional. The stored playlist.</param>
    /// <param name="entries">Optional. The requested page of entries. Defaults to a page of random entries.</param>
    /// <returns>The created <see cref="PlaylistDetailsDto"/>.</returns>
    public PlaylistDetailsDto Create(
        PlaylistEntity? playlist = null,
        PaginatedResultDto<PlaylistEntryDto>? entries = null)
    {
        List<PlaylistEntryDto> entryDtos = _playlistEntryDtoFixture.CreateMany();
        return new PlaylistDetailsDto(
            playlist ?? _playlistEntityFixture.Create(),
            entries ?? _paginatedResultDtoFixture.Create(data: entryDtos, count: entryDtos.Count));
    }
}
