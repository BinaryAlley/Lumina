#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteConsumptionItemHistoryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryEndpointSummary : Summary<DeleteConsumptionItemHistoryEndpoint, DeleteConsumptionItemHistoryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryEndpointSummary"/> class.
    /// </summary>
    public DeleteConsumptionItemHistoryEndpointSummary()
    {
        Summary = "Deletes every consumption record of a media item of the selected user.";
        Description = "Proxies the request that deletes every recorded consumption session of one media item of the selected user, in the library named by the request, to the API server. The deletion covers every session of the item, independent of any date range. When userId is omitted, the history of the current user is deleted; an administrator can name another user. Deleting an item that has no recorded session succeeds without changing anything. The resume point of the media item is not deleted. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        RequestParam(request => request.UserId, "The Id of the user whose history is deleted. Optional; when omitted, the current user is used.");
        RequestParam(request => request.MediaKind, "The kind of the consumed media item.");
        RequestParam(request => request.MediaItemId, "The Id of the consumed media item.");
        RequestParam(request => request.LibraryId, "The Id of the media library that contains the media item.");

        ResponseParam<SuccessResponse>(response => response.IsSuccess, "Whether the operation succeeded.");

        Response(200, "The consumption records of the media item were deleted, or a JSON error with success set to false.",
            example: new SuccessResponse(true)
        );
    }
}
