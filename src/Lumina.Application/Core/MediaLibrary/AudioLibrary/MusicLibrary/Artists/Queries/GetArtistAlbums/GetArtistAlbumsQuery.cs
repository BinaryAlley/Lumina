#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbums;

/// <summary>
/// Query for getting all the albums of an artist.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the artist belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist whose albums are retrieved, taken from the route.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistAlbumsQuery(
    string? LibraryId,
    string? ArtistId
) : IQuery;
