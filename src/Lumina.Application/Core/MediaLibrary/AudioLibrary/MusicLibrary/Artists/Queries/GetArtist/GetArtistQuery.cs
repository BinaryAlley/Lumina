#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;

/// <summary>
/// Query for getting an artist by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the artist belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist to get, taken from the route.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistQuery(
    string? LibraryId,
    string? ArtistId
) : IQuery;
