#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;

/// <summary>
/// Class used for providing a textual description for the <see cref="ClearConsumptionHistoryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryEndpointSummary : Summary<ClearConsumptionHistoryEndpoint, ClearConsumptionHistoryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryEndpointSummary"/> class.
    /// </summary>
    public ClearConsumptionHistoryEndpointSummary()
    {
        Summary = "Deletes every consumption record of the selected user.";
        Description = "Proxies the request that deletes every recorded consumption session of the selected user to the API server. When userId is omitted, the history of the current user is deleted; an administrator can name another user. Clearing an already empty history succeeds without changing anything. The resume points are not deleted. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        RequestParam(request => request.UserId, "The Id of the user whose history is deleted. Optional; when omitted, the current user is used.");

        ResponseParam<SuccessResponse>(response => response.IsSuccess, "Whether the operation succeeded.");

        Response(200, "The consumption history was cleared, or a JSON error with success set to false.",
            example: new SuccessResponse(true)
        );
    }
}
