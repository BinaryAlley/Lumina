#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to get the artists of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose artists are retrieved. Required.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of artists to retrieve per page. Optional.</param>
/// <param name="SearchTerm">The search term used to filter the artists by name. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetArtistsRequest(
    Guid LibraryId,
    int? CurrentPage,
    int? PerPage,
    string? SearchTerm
);
