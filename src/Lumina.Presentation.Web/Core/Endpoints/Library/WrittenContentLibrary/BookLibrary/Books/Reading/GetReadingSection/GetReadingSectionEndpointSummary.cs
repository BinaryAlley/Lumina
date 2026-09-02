#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingSectionEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionEndpointSummary : Summary<GetReadingSectionEndpoint, GetBookReadingSectionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpointSummary"/> class.
    /// </summary>
    public GetReadingSectionEndpointSummary()
    {
        Summary = "Retrieves the content of a reading section of a book.";
        Description = "Retrieves the sanitized HTML content of the reading section of a book, ready to be rendered by the client.";
        RequestParam(r => r.BookId, "The Id of the book whose reading section is retrieved. Required.");
        RequestParam(r => r.LocationRef, "The opaque location reference of the reading section. Required.");

        ExampleRequest = new GetBookReadingSectionRequest(
            BookId: Guid.NewGuid(),
            LocationRef: "chapter-1"
        );

        Response(200, "The content of the reading section is returned.");
    }
}
