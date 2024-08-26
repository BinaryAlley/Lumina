#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Models.Books;
using Lumina.Application.Common.Models.Contributors;
using Lumina.Domain.Common.Enums;
#endregion

namespace Lumina.Presentation.Api.Common.Contracts.Books;

/// <summary>
/// Represents a request to add a book.
/// </summary>
public record AddBookRequest(
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
    List<MediaContributorDto> Contributors,
    List<BookRatingDto> Ratings
);