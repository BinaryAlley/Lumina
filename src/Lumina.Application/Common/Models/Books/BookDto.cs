#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Models.Common;
#endregion

namespace Lumina.Application.Common.Models.Books;

/// <summary>
/// Represents a book.
/// </summary>
public class BookDto : IStorageEntity
{
    #region ==================================================================== PROPERTIES =================================================================================
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the media item.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Gets the original title of the media item.
    /// </summary>
    public string? OriginalTitle { get; init; }

    /// <summary>
    /// Gets the optional description of the media item.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the optional original release date of the media item.
    /// </summary>
    public DateOnly? OriginalReleaseDate { get; init; }

    /// <summary>
    /// Gets the optional original release year of the media item.
    /// This is useful when only the year of original release is known.
    /// </summary>
    public int? OriginalReleaseYear { get; init; }

    /// <summary>
    /// Gets the optional re-release or reissue date of the media item.
    /// </summary>
    public DateOnly? ReReleaseDate { get; init; }

    /// <summary>
    /// Gets the optional re-release or reissue year of the media item.
    /// </summary>
    public int? ReReleaseYear { get; init; }

    /// <summary>
    /// Gets the optional country or region of release.
    /// </summary>
    public string? ReleaseCountry { get; init; }

    /// <summary>
    /// Gets the optional release version or edition. (e.g. "Original", "Director's Cut", "2.0")
    /// </summary>
    public string? ReleaseVersion { get; init; }

    /// <summary>
    /// Gets the ISO 639-1 two-letter language code.
    /// </summary>
    public string? LanguageCode { get; init; }

    /// <summary>
    /// Gets the full name of the language in English.
    /// </summary>
    public string? LanguageName { get; init; }

    /// <summary>
    /// Gets an optional native name of the language.
    /// </summary>
    public string? LanguageNativeName { get; init; }

    /// <summary>
    /// Gets the optional ISO 639-1 two-letter original language code.
    /// </summary>
    public string? OriginalLanguageCode { get; init; }

    /// <summary>
    /// Gets the optional full name of the original language in English.
    /// </summary>
    public string? OriginalLanguageName { get; init; }

    /// <summary>
    /// Gets an optional native name of the original language.
    /// </summary>
    public string? OriginalLanguageNativeName { get; init; }

    /// <summary>
    /// Gets the list of tags associated with the media item.
    /// </summary>
    public HashSet<TagDto> Tags { get; set; } = [];

    /// <summary>
    /// Gets the list of genres associated with the media item.
    /// </summary>
    public HashSet<GenreDto> Genres { get; set; } = [];

    /// <summary>
    /// Gets the publisher of the written content.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// Gets the number of pages in the written content.
    /// </summary>
    public int? PageCount { get; init; }

    /// <summary>
    /// Gets the format of the book (e.g., Hardcover, Paperback), if applicable.
    /// </summary>
    public string Format { get; init; } = null!;

    /// <summary>
    /// Gets the edition of the book, if applicable.
    /// </summary>
    public string? Edition { get; init; }

    /// <summary>
    /// Gets the volume or book number in the series, if applicable.
    /// </summary>
    public int? VolumeNumber { get; init; }

    /// <summary>
    /// Gets the series id, if the book is part of a series.
    /// </summary>
    //public Guid? BookSeriesId { get; init; } = null;

    /// <summary>
    /// Gets the ASIN (Amazon Standard Identification Number) of the book.
    /// </summary>
    public string? ASIN { get; init; } = null!;

    /// <summary>
    /// Gets the Goodreads ID of the book.
    /// </summary>
    public string? GoodreadsId { get; init; } = null!;

    /// <summary>
    /// Gets the Library of Congress Control Number (LCCN) of the book.
    /// </summary>
    public string? LCCN { get; init; } = null!;

    /// <summary>
    /// Gets the OCLC Number (WorldCat identifier) of the book.
    /// </summary>
    public string? OCLCNumber { get; init; } = null!;

    /// <summary>
    /// Gets the Open Library ID of the book.
    /// </summary>
    public string? OpenLibraryId { get; init; } = null!;

    /// <summary>
    /// Gets the LibraryThing ID of the book.
    /// </summary>
    public string? LibraryThingId { get; init; } = null!;

    /// <summary>
    /// Gets the Google Books ID of the book.
    /// </summary>
    public string? GoogleBooksId { get; init; } = null!;

    /// <summary>
    /// Gets the Barnes & Noble ID of the book.
    /// </summary>
    public string? BarnesAndNobleId { get; init; } = null!;

    /// <summary>
    /// Gets the Apple Books ID of the book.
    /// </summary>
    public string? AppleBooksId { get; init; } = null!;

    /// <summary>
    /// Gets the list of ISBN (International Standard Book Number) of the book.
    /// </summary>
    public List<IsbnDto> ISBNs { get; set; } = [];

    /// <summary>
    /// Gets the list of media contributors (actors, directors, etc) starring in this book.
    /// </summary>
    //public List<ContributorIdDto> ContributorIds { get; init; } = [];

    /// <summary>
    /// Gets the list of ratings for this book.
    /// </summary>
    public List<BookRatingDto> Ratings { get; init; } = [];

    /// <summary>
    /// Gets the time and date when the entity was added.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? Updated { get; set; }
    #endregion
}