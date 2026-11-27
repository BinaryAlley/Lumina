#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.Read;

/// <summary>
/// Class used for providing a textual description for the <see cref="ReadViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadViewEndpointSummary : Summary<ReadViewEndpoint, ReadBookViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadViewEndpointSummary"/> class.
    /// </summary>
    public ReadViewEndpointSummary()
    {
        Summary = "Displays the reading view of a book.";
        Description = "Displays the reading view of the book identified by the route.";
        RequestParam(r => r.BookId, "The Id of the book to read. Required.");
        RequestParam(r => r.LibraryId, "The Id of the media library the book belongs to. Required.");

        ExampleRequest = new ReadBookViewRequest(
            BookId: Guid.NewGuid(),
            LibraryId: Guid.NewGuid()
        );

        Response(200, "The reading view of the book is displayed.");
    }
}
