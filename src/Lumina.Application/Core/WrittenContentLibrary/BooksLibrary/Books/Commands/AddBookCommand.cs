#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Models.Books;
using Lumina.Application.Common.Models.Contributors;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Mediator;
#endregion

namespace Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands;

/// <summary>
/// Command for adding a book.
/// </summary>
public record AddBookCommand(
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
    string? KoboId,
    string? AppleBooksId,
    List<IsbnDto> ISBNs,
    List<MediaContributorDto> Contributors,
    List<BookRatingDto> Ratings
) : IRequest<ErrorOr<Book>>;