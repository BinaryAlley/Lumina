#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Lightweight read model of an artist, containing only the fields needed to display the artist in a card-based grid or a list.
/// </summary>
/// <param name="Id">The Id of the artist.</param>
/// <param name="Name">The name of the artist.</param>
/// <param name="CoverPath">The path of the image representing the artist, if available.</param>
public sealed record ArtistLiteRow(
    Guid Id,
    string Name,
    string? CoverPath
);
