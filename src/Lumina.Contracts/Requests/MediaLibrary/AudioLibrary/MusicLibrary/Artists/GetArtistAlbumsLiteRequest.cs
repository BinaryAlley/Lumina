#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to get the lightweight read models of the albums of an artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist whose albums are retrieved.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of albums to retrieve per page. Optional.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistAlbumsLiteRequest(
    Guid ArtistId,
    int? CurrentPage,
    int? PerPage
);
