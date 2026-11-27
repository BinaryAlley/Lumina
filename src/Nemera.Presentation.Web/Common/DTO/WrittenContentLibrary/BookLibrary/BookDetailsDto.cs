#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object carrying the full details of a book, as stored in the media library.
/// </summary>
[DebuggerDisplay("Title: {Metadata?.Title}")]
public class BookDetailsDto
{
    /// <summary>
    /// Gets or sets the Id of the book.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Id of the media library this book belongs to.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the file system path of the book.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the written content metadata of the book.
    /// </summary>
    public WrittenContentMetadataDto? Metadata { get; set; }

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
    public float? VolumeNumber { get; set; }

    /// <summary>
    /// Gets or sets the ASIN (Amazon Standard Identification Number) of the book, if applicable.
    /// </summary>
    public string? ASIN { get; set; }

    /// <summary>
    /// Gets or sets the Goodreads Id of the book, if applicable.
    /// </summary>
    public string? GoodreadsId { get; set; }

    /// <summary>
    /// Gets or sets the Library of Congress Control Number (LCCN) of the book, if applicable.
    /// </summary>
    public string? LCCN { get; set; }

    /// <summary>
    /// Gets or sets the OCLC Number (WorldCat identifier) of the book, if applicable.
    /// </summary>
    public string? OCLCNumber { get; set; }

    /// <summary>
    /// Gets or sets the Open Library Id of the book, if applicable.
    /// </summary>
    public string? OpenLibraryId { get; set; }

    /// <summary>
    /// Gets or sets the LibraryThing Id of the book, if applicable.
    /// </summary>
    public string? LibraryThingId { get; set; }

    /// <summary>
    /// Gets or sets the Google Books Id of the book, if applicable.
    /// </summary>
    public string? GoogleBooksId { get; set; }

    /// <summary>
    /// Gets or sets the Barnes &amp; Noble Id of the book, if applicable.
    /// </summary>
    public string? BarnesAndNobleId { get; set; }

    /// <summary>
    /// Gets or sets the Apple Books Id of the book, if applicable.
    /// </summary>
    public string? AppleBooksId { get; set; }

    /// <summary>
    /// Gets or sets the list of ISBN (International Standard Book Number) of the book.
    /// </summary>
    public List<IsbnDto>? ISBNs { get; set; }

    /// <summary>
    /// Gets or sets the list of media contributors of the book.
    /// </summary>
    public List<MediaContributorDto>? Contributors { get; set; }

    /// <summary>
    /// Gets or sets the list of ratings of the book.
    /// </summary>
    public List<BookRatingDto>? Ratings { get; set; }

    /// <summary>
    /// Gets or sets the status of the metadata enrichment of the book.
    /// </summary>
    public string? MetadataStatus { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the metadata of the book was last enriched, if applicable.
    /// </summary>
    public DateTime? LastMetadataUpdateUtc { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin that enriched the metadata of the book, if applicable.
    /// </summary>
    public string? MetadataProvider { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the book was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional date and time when the book was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the path of the cover image of the book, if available.
    /// </summary>
    public string? CoverPath { get; set; }
}
