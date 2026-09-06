#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Represents a request to update an existing book.
/// </summary>
[DebuggerDisplay("Title: {Metadata?.Title}")]
public class UpdateBookRequest
{
    /// <summary>
    /// Gets or sets the Id of the book to update, taken from the route.
    /// </summary>
    public string? Id { get; set; }

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
    /// Gets or sets the series name, if the book is part of a series.
    /// </summary>
    public BookSeriesDto? Series { get; set; }

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
}
