#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Playlists;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="PlaylistEntryDto"/>.
/// </summary>
public static class PlaylistEntryDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="PlaylistEntryResponse"/>.
    /// </summary>
    /// <param name="data">The playlist entry data to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PlaylistEntryResponse ToResponse(this PlaylistEntryDto data)
    {
        if (data.Display is null)
            return new PlaylistEntryResponse(
                data.Entry.Id,
                data.Entry.MediaItemId,
                data.Entry.MediaItemType,
                data.DisplayPosition,
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                null);

        PlaylistEntryDisplayDto display = data.Display;
        return new PlaylistEntryResponse(
            data.Entry.Id,
            data.Entry.MediaItemId,
            data.Entry.MediaItemType,
            data.DisplayPosition,
            display.LibraryId,
            display.AlbumId,
            display.ArtistId,
            display.TrackTitle,
            display.AlbumTitle,
            display.ArtistName,
            display.DurationInSeconds,
            display.CoverPath);
    }

    /// <summary>
    /// Converts <paramref name="data"/> to a paginated collection of <see cref="PlaylistEntryResponse"/>.
    /// </summary>
    /// <param name="data">The paginated playlist entry data to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<PlaylistEntryResponse> ToResponses(this PaginatedResultDto<PlaylistEntryDto> data)
    {
        return new PaginatedResponse<PlaylistEntryResponse>
        {
            Data = [.. data.Data.Select(entry => entry.ToResponse())],
            CurrentPage = data.CurrentPage,
            PerPage = data.PerPage,
            Count = data.Count,
            NumberOfPages = data.NumberOfPages
        };
    }
}
