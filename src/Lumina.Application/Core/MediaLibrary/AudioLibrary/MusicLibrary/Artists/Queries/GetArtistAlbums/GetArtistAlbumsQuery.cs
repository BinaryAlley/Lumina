#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbums;

/// <summary>
/// Query for getting all the albums of an artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist whose albums are retrieved.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistAlbumsQuery(
    Guid ArtistId
) : IQuery;
