#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;

/// <summary>
/// Query for getting the lightweight read models of all the albums of an artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist whose albums are retrieved.</param>
/// <param name="CurrentPage">The page of results to retrieve.</param>
/// <param name="PerPage">The maximum number of albums to retrieve per page.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistAlbumsLiteQuery(
    Guid ArtistId,
    int? CurrentPage,
    int? PerPage
) : IQuery;
