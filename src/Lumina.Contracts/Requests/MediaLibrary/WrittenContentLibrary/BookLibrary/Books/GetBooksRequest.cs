#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to get the books of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose books are retrieved. Required.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of books to retrieve per page. Optional.</param>
/// <param name="SearchTerm">The search term used to filter results. Optional.</param>
/// <param name="SortBy">The name of the field by which to sort the results. Optional.</param>
/// <param name="SortOrder">The direction in which to sort the results. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetBooksRequest(
    Guid LibraryId,
    int? CurrentPage,
    int? PerPage,
    string? SearchTerm,
    string? SortBy,
    SortOrder? SortOrder
);
