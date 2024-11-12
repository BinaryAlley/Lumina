#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddBookEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryEndpointSummary : Summary<AddBookEndpoint, AddBookRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpointSummary"/> class.
    /// </summary>
    public AddLibraryEndpointSummary()
    {
        Summary = "Adds a new book.";
        Description = "Creates a new book and returns its details, including the location of the newly created resource.";

        RequestParam(r => r.Metadata, "Written content metadata of the book.");
        RequestParam(r => r.Metadata!.Title, "The title of the written content.");
        RequestParam(r => r.Format, "The format of the book (e.g., Hardcover, Paperback), if applicable.");
        RequestParam(r => r.Edition, "The edition of the book, if applicable.");
        RequestParam(r => r.VolumeNumber, "The volume or book number in the series, if applicable.");
        RequestParam(r => r.Series, "The series name, if the book is part of a series.");
        RequestParam(r => r.ASIN, "The ASIN (Amazon Standard Identification Number) of the book, if applicable.");
        RequestParam(r => r.GoodreadsId, "The Goodreads ID of the book, if applicable.");
        RequestParam(r => r.LCCN, "The Library of Congress Control Number (LCCN) of the book, if applicable.");
        RequestParam(r => r.OCLCNumber, "The OCLC Number (WorldCat identifier) of the book, if applicable.");
        RequestParam(r => r.OpenLibraryId, "The Open Library ID of the book, if applicable.");
        RequestParam(r => r.LibraryThingId, "The LibraryThing ID of the book, if applicable.");
        RequestParam(r => r.GoogleBooksId, "The Google Books ID of the book, if applicable.");
        RequestParam(r => r.BarnesAndNobleId, "The Barnes & Noble ID of the book, if applicable.");
        RequestParam(r => r.AppleBooksId, "The Apple Books ID of the book, if applicable.");
        RequestParam(r => r.ISBNs, "The list of ISBN (International Standard Book Number) of the book.");
        RequestParam(r => r.Contributors, "The list of media contributors (actors, directors, etc) starring in this book.");
        RequestParam(r => r.Ratings, "The list of ratings for this book.");

        ResponseParam<BookResponse>(r => r.Id, "The unique identifier of the entity.");
        ResponseParam<BookResponse>(r => r.Created, "The date and time when the entity was created.");
        ResponseParam<BookResponse>(r => r.Updated, "The date and time when the entity was last updated.");
        ResponseParam<BookResponse>(r => r.Metadata, "The written content metadata of the book.");
        ResponseParam<BookResponse>(r => r.Metadata.Title, "The title of the media item.");
        ResponseParam<BookResponse>(r => r.Metadata.OriginalTitle, "The original title of the media item.");
        Response<BookResponse>(201, "The book was successfully created.", "The location of the created resource is provided in the Location header.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
