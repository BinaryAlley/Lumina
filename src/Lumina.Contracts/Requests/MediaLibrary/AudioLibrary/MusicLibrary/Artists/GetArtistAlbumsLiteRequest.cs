#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to get the lightweight read models of the albums of an artist.
/// </summary>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of albums to retrieve per page. Optional.</param>
[DebuggerDisplay("CurrentPage: {CurrentPage}, PerPage: {PerPage}")]
public record GetArtistAlbumsLiteRequest(
    int? CurrentPage,
    int? PerPage
);
