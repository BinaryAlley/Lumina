#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to get the lightweight read models of the artists of a media library.
/// </summary>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of artists to retrieve per page. Optional.</param>
/// <param name="SearchTerm">The search term used to filter the artists by name. Optional.</param>
[DebuggerDisplay("SearchTerm: {SearchTerm}")]
public record GetArtistsLiteRequest(
    int? CurrentPage,
    int? PerPage,
    string? SearchTerm
);
