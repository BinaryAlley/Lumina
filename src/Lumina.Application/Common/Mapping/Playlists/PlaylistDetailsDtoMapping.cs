#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Contracts.Responses.Playlists;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="PlaylistDetailsDto"/>.
/// </summary>
public static class PlaylistDetailsDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="PlaylistResponse"/>.
    /// </summary>
    /// <param name="data">The playlist details data to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PlaylistResponse ToResponse(this PlaylistDetailsDto data)
    {
        return new PlaylistResponse(
            data.Playlist.Id,
            data.Playlist.Name,
            data.Playlist.Description,
            data.Playlist.Type,
            [.. data.Entries.Select(entry => entry.ToResponse())],
            data.Playlist.EntryCount,
            data.Playlist.CreatedOnUtc,
            data.Playlist.UpdatedOnUtc);
    }
}
