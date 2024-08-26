#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Models.Common;
#endregion

namespace Lumina.Application.Common.Models.Books;

/// <summary>
/// Represents a book.
/// </summary>
public class BookDto : IStorageEntity
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? OriginalTitle { get; init; }
    public string? Description { get; init; }
    public DateOnly? OriginalReleaseDate { get; init; }
    public int? OriginalReleaseYear { get; init; }
    public DateOnly? ReReleaseDate { get; init; }
    public int? ReReleaseYear { get; init; }
    public string? ReleaseCountry { get; init; }
    public string? ReleaseVersion { get; init; }
    public string? LanguageCode { get; init; }
    public string? LanguageName { get; init; }
    public string? LanguageNativeName { get; init; }
    public string? OriginalLanguageCode { get; init; }
    public string? OriginalLanguageName { get; init; }
    public string? OriginalLanguageNativeName { get; init; }
    public List<TagDto> Tags { get; init; } = [];
    public List<GenreDto> Genres { get; init; } = [];
    public string? Publisher { get; init; }
    public int? PageCount { get; init; }
    public string Format { get; init; } = null!;
    public string? Edition { get; init; }
    public int? VolumeNumber { get; init; }
    //public Guid? BookSeriesId { get; init; } = null;
    public string? ASIN { get; init; } = null!;
    public string? GoodreadsId { get; init; } = null!;
    public string? LCCN { get; init; } = null!;
    public string? OCLCNumber { get; init; } = null!;
    public string? OpenLibraryId { get; init; } = null!;
    public string? LibraryThingId { get; init; } = null!;
    public string? GoogleBooksId { get; init; } = null!;
    public string? BarnesAndNobleId { get; init; } = null!;
    public string? AppleBooksId { get; init; } = null!;
    public List<IsbnDto> ISBNs { get; init; } = [];
    //public List<ContributorIdDto> ContributorIds { get; init; } = [];
    public List<BookRatingDto> Ratings { get; init; } = [];
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
}