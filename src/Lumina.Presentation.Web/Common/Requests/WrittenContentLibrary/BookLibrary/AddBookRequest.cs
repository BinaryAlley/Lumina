#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using Lumina.Presentation.Web.Common.Models.MediaContributors;
using Lumina.Presentation.Web.Common.Models.WrittenContentLibrary;
using Lumina.Presentation.Web.Common.Models.WrittenContentLibrary.BookLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a request to add a book.
/// </summary>
public class AddBookRequest
{
    /// <summary>
    /// Gets or sets the written content metadata of the book.
    /// </summary>
    public WrittenContentMetadataModel? Metadata { get; set; }

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

    /// <summary>
    /// Gets or sets the series name, if the book is part of a series.
    /// </summary>
    public BookSeriesModel? Series { get; set; }

    /// <summary>
    /// Gets or sets the ASIN (Amazon Standard Identification Number) of the book, if applicable.
    /// </summary>
    public string? ASIN { get; set; }

    /// <summary>
    /// Gets or sets the Goodreads ID of the book, if applicable.
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
    /// Gets or sets the Open Library ID of the book, if applicable.
    /// </summary>
    public string? OpenLibraryId { get; set; }

    /// <summary>
    /// Gets or sets the LibraryThing ID of the book, if applicable.
    /// </summary>
    public string? LibraryThingId { get; set; }

    /// <summary>
    /// Gets or sets the Google Books ID of the book, if applicable.
    /// </summary>
    public string? GoogleBooksId { get; set; }

    /// <summary>
    /// Gets or sets the Barnes & Noble ID of the book, if applicable.
    /// </summary>
    public string? BarnesAndNobleId { get; set; }

    /// <summary>
    /// Gets or sets the Apple Books ID of the book, if applicable.
    /// </summary>
    public string? AppleBooksId { get; set; }

    /// <summary>
    /// Gets or sets the list of ISBN (International Standard Book Number) of the book.
    /// </summary>
    public List<IsbnModel>? ISBNs { get; set; }

    /// <summary>
    /// Gets or sets the list of media contributors (actors, directors, etc) starring in this book.
    /// </summary>
    public List<MediaContributorModel>? Contributors { get; set; }

    /// <summary>
    /// Gets or sets the list of ratings for this book.
    /// </summary>
    public List<BookRatingModel>? Ratings { get; set; }
}