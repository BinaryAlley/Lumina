#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Requests.Libraries;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetLibraryItems;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryItemsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryItemsEndpointSummary : Summary<GetLibraryItemsEndpoint, GetBooksLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryItemsEndpointSummary"/> class.
    /// </summary>
    public GetLibraryItemsEndpointSummary()
    {
        Summary = "Retrieves the lightweight details of the books of a media library.";
        Description = "Retrieves the lightweight details of the books of the media library identified by the request.";

        RequestParam(r => r.LibraryId, "The Id of the media library whose books are retrieved.");
        RequestParam(r => r.CurrentPage, "Optional. The page of results to retrieve.");
        RequestParam(r => r.PerPage, "Optional. The maximum number of books to retrieve per page.");
        RequestParam(r => r.SearchTerm, "Optional. The search term used to filter results.");
        RequestParam(r => r.FilterAlphaKey, "Optional. The alpha key used to filter the results by the first character of their title, for the alpha picker.");
        RequestParam(r => r.IgnoreThePrefixForAlphaPicker, "Whether the leading The prefix of a title is ignored when computing the alpha key, or not.");
        RequestParam(r => r.SortBy, "Optional. The name of the field by which to sort the results.");
        RequestParam(r => r.SortOrder, "Optional. The direction in which to sort the results.");

        Response(200, "The lightweight details of the books of the media library are returned.", example: new SuccessResponse<PaginatedBookLiteDto>(true, default));
    }
}
