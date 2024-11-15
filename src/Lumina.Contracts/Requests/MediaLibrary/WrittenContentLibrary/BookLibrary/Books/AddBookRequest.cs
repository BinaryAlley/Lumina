#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to add a book.
/// </summary>
/// <param name="Metadata">Written content metadata of the book. Required.</param>
/// <param name="Format">The format of the book (e.g., Hardcover, Paperback). Optional.</param>
/// <param name="Edition">The edition of the book. Optional.</param>
/// <param name="VolumeNumber">The volume or book number in the series. Optional.</param>
/// <param name="Series">The series name, if the book is part of a series. Optional.</param>
/// <param name="ASIN">The ASIN (Amazon Standard Identification Number) of the book. Optional.</param>
/// <param name="GoodreadsId">The Goodreads Id of the book. Optional.</param>
/// <param name="LCCN">The Library of Congress Control Number (LCCN) of the book. Optional.</param>
/// <param name="OCLCNumber">The OCLC Number (WorldCat identifier) of the book. Optional.</param>
/// <param name="OpenLibraryId">The Open Library Id of the book. Optional.</param>
/// <param name="LibraryThingId">The LibraryThing Id of the book. Optional.</param>
/// <param name="GoogleBooksId">The Google Books Id of the book. Optional.</param>
/// <param name="BarnesAndNobleId">The Barnes & Noble Id of the book. Optional.</param>
/// <param name="AppleBooksId">The Apple Books Id of the book. Optional.</param>
/// <param name="ISBNs">The list of ISBN (International Standard Book Number) of the book. Required.</param>
/// <param name="Contributors">The list of media contributors (actors, directors, etc) starring in this book. Required.</param>
/// <param name="Ratings">The list of ratings for this book. Required.</param>
[DebuggerDisplay("Title: {Metadata.Title}")]
public record AddBookRequest(
    WrittenContentMetadataDto? Metadata,
    BookFormat? Format,
    string? Edition,
    int? VolumeNumber,
    BookSeriesDto? Series,
    string? ASIN,
    string? GoodreadsId,
    string? LCCN,
    string? OCLCNumber,
    string? OpenLibraryId,
    string? LibraryThingId,
    string? GoogleBooksId,
    string? BarnesAndNobleId,
    string? AppleBooksId,
    List<IsbnDto>? ISBNs,
    List<MediaContributorDto>? Contributors,
    List<BookRatingDto>? Ratings
);
