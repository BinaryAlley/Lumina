#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeletedBooks;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeletedBooksViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedBooksViewEndpointSummary : Summary<DeletedBooksViewEndpoint, GetBooksViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedBooksViewEndpointSummary"/> class.
    /// </summary>
    public DeletedBooksViewEndpointSummary()
    {
        Summary = "Renders the deleted items page of a book library.";
        Description = "Renders the page that lists the books the user deleted from a book library and lets the user restore them.";

        Response(200, "The deleted items page is rendered.");
    }
}
