#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request to get the lightweight details of the artists of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose artists are retrieved. Required.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of artists to retrieve per page. Optional.</param>
/// <param name="SearchTerm">The search term used to filter the artists by name. Optional.</param>
/// <param name="FilterAlphaKey">
/// The alpha key used to filter the results by the first character of their name, for the alpha picker. Optional.
/// Can be <see langword="null"/> to disable alpha filtering, a single ASCII letter (case-insensitive) to filter by that letter,
/// "#" to filter by names whose first character is a digit, or "*" to filter by names whose first character is neither a letter nor a digit.
/// </param>
/// <param name="ShouldIgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix of a name is ignored when computing the alpha key, or not. Optional.</param>
/// <param name="SortBy">The name of the field by which to sort the results. Optional.</param>
/// <param name="SortOrder">The direction in which to sort the results. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetMusicArtistsLiteRequest(
    Guid LibraryId,
    int? CurrentPage,
    int? PerPage,
    string? SearchTerm,
    string? FilterAlphaKey,
    bool ShouldIgnoreThePrefixForAlphaPicker,
    string? SortBy,
    SortOrder? SortOrder
);
