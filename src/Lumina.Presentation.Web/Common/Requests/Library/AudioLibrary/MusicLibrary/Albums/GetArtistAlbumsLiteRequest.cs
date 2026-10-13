#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request to get the lightweight details of the albums of a music artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist whose albums are retrieved, taken from the route. Required.</param>
/// <param name="LibraryId">The unique identifier of the media library the artist belongs to. Required.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of albums to retrieve per page. Optional.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistAlbumsLiteRequest(
    string ArtistId,
    Guid LibraryId,
    int? CurrentPage,
    int? PerPage
);
