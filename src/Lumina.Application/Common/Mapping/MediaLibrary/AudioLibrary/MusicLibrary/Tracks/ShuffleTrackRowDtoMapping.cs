#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="ShuffleTrackRowDto"/>.
/// </summary>
public static class ShuffleTrackRowDtoMapping
{
    /// <summary>
    /// Converts <paramref name="readModel"/> to <see cref="ShuffleTrackResponse"/>.
    /// </summary>
    /// <param name="readModel">The read model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ShuffleTrackResponse ToResponse(this ShuffleTrackRowDto readModel)
    {
        return new ShuffleTrackResponse(
            readModel.TrackId,
            readModel.Title,
            readModel.DurationInSeconds,
            readModel.AlbumId,
            readModel.AlbumTitle,
            readModel.ArtistId,
            readModel.CoverPath);
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a collection of <see cref="ShuffleTrackResponse"/>.
    /// </summary>
    /// <param name="readModels">The read models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<ShuffleTrackResponse> ToResponses(this IEnumerable<ShuffleTrackRowDto> readModels)
    {
        return [.. readModels.Select(readModel => readModel.ToResponse())];
    }
}
