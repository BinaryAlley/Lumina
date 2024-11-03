#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetBooksEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetBooksEndpointSummary : Summary<GetBooksEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpointSummary"/> class.
    /// </summary>
    public GetBooksEndpointSummary()
    {
        Summary = "Retrieves the list of books.";
        Description = "Returns the entire list of books.";

        Response<IEnumerable<BookResponse>>(200, "The list of books is returned.");
    }
}
