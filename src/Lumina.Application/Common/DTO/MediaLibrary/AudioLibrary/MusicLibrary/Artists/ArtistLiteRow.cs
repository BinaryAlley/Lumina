#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Lightweight read model of an artist, containing only the fields needed to display the artist in a card-based grid or a list.
/// </summary>
public sealed record ArtistLiteRow
{
    /// <summary>
    /// Gets the Id of the artist.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the artist.
    /// </summary>
    public required string Name { get; init; }
}
