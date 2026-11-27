#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;

/// <summary>
/// Class used for providing a textual description for the <see cref="RestoreBookEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreBookEndpointSummary : Summary<RestoreBookEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBookEndpointSummary"/> class.
    /// </summary>
    public RestoreBookEndpointSummary()
    {
        Summary = "Restores a deleted book.";
        Description = "Restores the deleted book identified by the route to the catalogue of its library, together with the metadata and artwork that were kept while it was deleted.";

        Response(200, "The book was restored.");
    }
}
