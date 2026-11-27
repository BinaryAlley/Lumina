#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeleteBook;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteBookEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteBookEndpointSummary : Summary<DeleteBookEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookEndpointSummary"/> class.
    /// </summary>
    public DeleteBookEndpointSummary()
    {
        Summary = "Deletes a book.";
        Description = "Moves the book identified by the route to the deleted items of its library, where its metadata and stored artwork are kept so that the deletion can be undone.";

        Response(200, "The book was moved to the deleted items of its library.");
    }
}
