#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Models.MediaContributors;
using Lumina.Contracts.Models.WrittenContentLibrary;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Contracts.Requests.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a request to add a book.
/// </summary>
/// <param name="Metadata">Written content metadata of the book.</param>
/// <param name="Format">The format of the book (e.g., Hardcover, Paperback), if applicable.</param>
/// <param name="Edition">The edition of the book, if applicable.</param>
/// <param name="VolumeNumber">The volume or book number in the series, if applicable.</param>
/// <param name="Series">The series name, if the book is part of a series.</param>
/// <param name="ASIN">The ASIN (Amazon Standard Identification Number) of the book, if applicable.</param>
/// <param name="GoodreadsId">The Goodreads ID of the book, if applicable.</param>
/// <param name="LCCN">The Library of Congress Control Number (LCCN) of the book, if applicable.</param>
/// <param name="OCLCNumber">The OCLC Number (WorldCat identifier) of the book, if applicable.</param>
/// <param name="OpenLibraryId">The Open Library ID of the book, if applicable.</param>
/// <param name="LibraryThingId">The LibraryThing ID of the book, if applicable.</param>
/// <param name="GoogleBooksId">The Google Books ID of the book, if applicable.</param>
/// <param name="BarnesAndNobleId">The Barnes & Noble ID of the book, if applicable.</param>
/// <param name="AppleBooksId">The Apple Books ID of the book, if applicable.</param>
/// <param name="ISBNs">The list of ISBN (International Standard Book Number) of the book.</param>
/// <param name="Contributors">The list of media contributors (actors, directors, etc) starring in this book.</param>
/// <param name="Ratings">The list of ratings for this book.</param>
public record AddBookRequest(
    WrittenContentMetadataModel? Metadata,
    BookFormat? Format,
    string? Edition,
    int? VolumeNumber,
    BookSeriesModel? Series,
    string? ASIN,
    string? GoodreadsId,
    string? LCCN,
    string? OCLCNumber,
    string? OpenLibraryId,
    string? LibraryThingId,
    string? GoogleBooksId,
    string? BarnesAndNobleId,
    string? AppleBooksId,
    List<IsbnModel>? ISBNs,
    List<MediaContributorModel>? Contributors,
    List<BookRatingModel>? Ratings
);