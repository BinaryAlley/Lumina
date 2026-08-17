#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;

/// <summary>
/// Class used for providing a textual description for the <see cref="BooksIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksIndexViewEndpointSummary : Summary<BooksIndexViewEndpoint, GetBooksViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BooksIndexViewEndpointSummary"/> class.
    /// </summary>
    public BooksIndexViewEndpointSummary()
    {
        Summary = "Renders the books browsing view.";
        Description = "Renders the view for browsing the books of the media library identified by the request.";

        RequestParam(r => r.LibraryId, "Optional. The unique identifier of the media library whose books are browsed.");

        Response(200, "The view for browsing the books of the media library is rendered.");
    }
}
