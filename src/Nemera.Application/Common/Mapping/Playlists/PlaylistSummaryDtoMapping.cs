#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Contracts.Responses.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="PlaylistSummaryDto"/>.
/// </summary>
public static class PlaylistSummaryDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="PlaylistSummaryResponse"/>.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PlaylistSummaryResponse ToResponse(this PlaylistSummaryDto dto)
    {
        return new PlaylistSummaryResponse(
            dto.Id,
            dto.Name,
            dto.Description,
            dto.Type,
            dto.EntryCount,
            dto.CreatedOnUtc,
            dto.UpdatedOnUtc);
    }
}
