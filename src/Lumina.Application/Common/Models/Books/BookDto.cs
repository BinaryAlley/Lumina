#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Enums;
#endregion

namespace Lumina.Application.Common.Models.Books;

/// <summary>
/// Represents a book.
/// </summary>
public record BookDto(
    WrittenContentMetadataDto Metadata,
    BookFormat Format,
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
    List<IsbnDto> ISBNs,
    List<Guid> Contributors,
    List<BookRatingDto> Ratings
);