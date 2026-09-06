#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetBookDetailsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookDetailsEndpointSummary : Summary<GetBookDetailsEndpoint, GetBookRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookDetailsEndpointSummary"/> class.
    /// </summary>
    public GetBookDetailsEndpointSummary()
    {
        Summary = "Gets the details of a book.";
        Description = "Gets the full details of the book identified by the request, for the details and editing view.";

        RequestParam(r => r.Id, "The unique identifier of the book to get. Required.");

        ExampleRequest = new GetBookRequest(
            Id: Guid.NewGuid().ToString()
        );

        Response(200, "The details of the book are returned.", example: new SuccessResponse<BookDetailsDto>(true, default));
    }
}
