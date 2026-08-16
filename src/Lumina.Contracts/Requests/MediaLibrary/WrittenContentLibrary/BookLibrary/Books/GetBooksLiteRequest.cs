#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to get the lightweight details of the books of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose books are retrieved. Required.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of books to retrieve per page. Optional.</param>
/// <param name="SearchTerm">The search term used to filter results. Optional.</param>
/// <param name="FilterAlphaKey">
/// The alpha key used to filter the results by the first character of their title, for the alpha picker. Optional.
/// Can be <see langword="null"/> to disable alpha filtering, a single ASCII letter (case-insensitive) to filter by that letter,
/// "#" to filter by titles whose first character is a digit, or "*" to filter by titles whose first character is neither a letter nor a digit.
/// </param>
/// <param name="IgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix of a title should be ignored when computing the alpha key, or not.</param>
/// <param name="SortBy">The name of the field by which to sort the results. Optional.</param>
/// <param name="SortOrder">The direction in which to sort the results. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetBooksLiteRequest(
    Guid LibraryId,
    int? CurrentPage,
    int? PerPage,
    string? SearchTerm,
    string? FilterAlphaKey,
    bool IgnoreThePrefixForAlphaPicker,
    string? SortBy,
    SortOrder? SortOrder
);
