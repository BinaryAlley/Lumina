#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request to get the lightweight read models of the tracks of an album.
/// </summary>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of tracks to retrieve per page. Optional.</param>
[DebuggerDisplay("CurrentPage: {CurrentPage}, PerPage: {PerPage}")]
public record GetAlbumTracksLiteRequest(
    int? CurrentPage,
    int? PerPage
);
