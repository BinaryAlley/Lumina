#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateBookCoverEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverEndpointSummary : Summary<UpdateBookCoverEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpointSummary"/> class.
    /// </summary>
    public UpdateBookCoverEndpointSummary()
    {
        Summary = "Updates the cover image of a book.";
        Description = "Updates the cover image of the book identified by the request, with the image uploaded in the multipart form of the request.";

        Response(200, "The relative path of the stored cover image is returned.", example: new SuccessResponse<string>(true, default));
    }
}
