#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.EditBook;

/// <summary>
/// Class used for providing a textual description for the <see cref="EditBookViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditBookViewEndpointSummary : Summary<EditBookViewEndpoint, GetBookRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditBookViewEndpointSummary"/> class.
    /// </summary>
    public EditBookViewEndpointSummary()
    {
        Summary = "Renders the edit book view.";
        Description = "Renders the view for editing the book identified by the route.";

        RequestParam(r => r.BookId, "The unique identifier of the book to edit. Required.");
        RequestParam(r => r.LibraryId, "The Id of the media library the book belongs to. Required.");

        ExampleRequest = new GetBookRequest(
            BookId: Guid.NewGuid().ToString(),
            LibraryId: Guid.NewGuid()
        );

        Response(200, "The view for editing the book is rendered.");
    }
}
