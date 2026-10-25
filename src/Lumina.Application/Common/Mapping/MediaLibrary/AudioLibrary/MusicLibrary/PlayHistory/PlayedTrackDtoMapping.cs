#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="PlayedTrackDto"/>.
/// </summary>
public static class PlayedTrackDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="PlayedTrackResponse"/>.
    /// </summary>
    /// <param name="data">The played track data to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PlayedTrackResponse ToResponse(this PlayedTrackDto data)
    {
        return new PlayedTrackResponse(
            data.TrackId,
            data.LibraryId,
            data.AlbumId,
            data.ArtistId,
            data.Title,
            data.AlbumTitle,
            data.ArtistName,
            data.CoverPath,
            // The repository reads the stored play time through SQLite, which returns it as an unspecified kind, and the JSON response must not
            // present a UTC wall clock as a local time. The value is UTC when it was stored, so it is marked as such before it is serialized.
            DateTime.SpecifyKind(data.LastPlayedOnUtc, DateTimeKind.Utc),
            data.PlayCount,
            data.CompletedPlayCount,
            data.ListenedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds
        );
    }

    /// <summary>
    /// Converts <paramref name="data"/> to a paginated collection of <see cref="PlayedTrackResponse"/>.
    /// </summary>
    /// <param name="data">The paginated played track data to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<PlayedTrackResponse> ToResponses(this PaginatedResultDto<PlayedTrackDto> data)
    {
        return new PaginatedResponse<PlayedTrackResponse>
        {
            Data = [.. data.Data.Select(track => track.ToResponse())],
            CurrentPage = data.CurrentPage,
            PerPage = data.PerPage,
            Count = data.Count,
            NumberOfPages = data.NumberOfPages
        };
    }
}
