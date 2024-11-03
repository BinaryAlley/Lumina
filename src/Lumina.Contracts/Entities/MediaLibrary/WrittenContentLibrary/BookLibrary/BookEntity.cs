#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.Common;
using Lumina.Contracts.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Repository entity for a book.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class BookEntity : IStorageEntity
{
    /// <summary>
    /// Gets the Id of the media item.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the title of the media item.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the original title of the media item.
    /// </summary>
    public string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets the optional description of the media item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional original release date of the media item.
    /// </summary>
    public DateOnly? OriginalReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the optional original release year of the media item.
    /// This is useful when only the year of original release is known.
    /// </summary>
    public int? OriginalReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the optional re-release or reissue date of the media item.
    /// </summary>
    public DateOnly? ReReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the optional re-release or reissue year of the media item.
    /// </summary>
    public int? ReReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the optional country or region of release.
    /// </summary>
    public string? ReleaseCountry { get; set; }

    /// <summary>
    /// Gets or sets the optional release version or edition. (e.g. "Original", "Director's Cut", "2.0")
    /// </summary>
    public string? ReleaseVersion { get; set; }

    /// <summary>
    /// Gets or sets the ISO 639-1 two-letter language code.
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the full name of the language in English.
    /// </summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// Gets or sets an optional native name of the language.
    /// </summary>
    public string? LanguageNativeName { get; set; }

    /// <summary>
    /// Gets or sets the optional ISO 639-1 two-letter original language code.
    /// </summary>
    public string? OriginalLanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the optional full name of the original language in English.
    /// </summary>
    public string? OriginalLanguageName { get; set; }

    /// <summary>
    /// Gets or sets an optional native name of the original language.
    /// </summary>
    public string? OriginalLanguageNativeName { get; set; }

    /// <summary>
    /// Gets or sets the list of tags associated with the media item.
    /// </summary>
    public HashSet<TagEntity> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of genres associated with the media item.
    /// </summary>
    public HashSet<GenreEntity> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets the publisher of the written content.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the number of pages in the written content.
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Gets or sets the format of the book (e.g., Hardcover, Paperback), if applicable.
    /// </summary>
    public BookFormat? Format { get; set; }

    /// <summary>
    /// Gets or sets the edition of the book, if applicable.
    /// </summary>
    public string? Edition { get; set; }

    /// <summary>
    /// Gets or sets the volume or book number in the series, if applicable.
    /// </summary>
    public int? VolumeNumber { get; set; }
    // TODO: uncomment when series is implemented
    /// <summary>
    /// Gets or sets the series id, if the book is part of a series.
    /// </summary>
    //public Guid? BookSeriesId { get; set; }

    /// <summary>
    /// Gets or sets the ASIN (Amazon Standard Identification Number) of the book.
    /// </summary>
    public string? ASIN { get; set; }

    /// <summary>
    /// Gets or sets the Goodreads ID of the book.
    /// </summary>
    public string? GoodreadsId { get; set; }

    /// <summary>
    /// Gets or sets the Library of Congress Control Number (LCCN) of the book.
    /// </summary>
    public string? LCCN { get; set; }

    /// <summary>
    /// Gets or sets the OCLC Number (WorldCat identifier) of the book.
    /// </summary>
    public string? OCLCNumber { get; set; }

    /// <summary>
    /// Gets or sets the Open Library ID of the book.
    /// </summary>
    public string? OpenLibraryId { get; set; }

    /// <summary>
    /// Gets or sets the LibraryThing ID of the book.
    /// </summary>
    public string? LibraryThingId { get; set; }

    /// <summary>
    /// Gets or sets the Google Books ID of the book.
    /// </summary>
    public string? GoogleBooksId { get; set; }

    /// <summary>
    /// Gets or sets the Barnes & Noble ID of the book.
    /// </summary>
    public string? BarnesAndNobleId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Apple Books ID of the book.
    /// </summary>
    public string? AppleBooksId { get; set; }

    /// <summary>
    /// Gets or sets the list of ISBN (International Standard Book Number) of the book.
    /// </summary>
    public List<IsbnEntity> ISBNs { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of media contributors (actors, directors, etc) starring in this book.
    /// </summary>
    //public List<ContributorIdModel> ContributorIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of ratings for this book.
    /// </summary>
    public List<BookRatingEntity> Ratings { get; set; } = [];

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? Updated { get; set; }
}
