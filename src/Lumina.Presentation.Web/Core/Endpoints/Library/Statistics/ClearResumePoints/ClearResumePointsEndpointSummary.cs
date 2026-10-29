#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.ClearResumePoints;

/// <summary>
/// Class used for providing a textual description for the <see cref="ClearResumePointsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsEndpointSummary : Summary<ClearResumePointsEndpoint, ClearResumePointsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsEndpointSummary"/> class.
    /// </summary>
    public ClearResumePointsEndpointSummary()
    {
        Summary = "Deletes the resume points of the selected user.";
        Description = "Proxies the request that deletes the resume points of the selected user, optionally limited to one library, to the API server. When userId is omitted, the resume points of the current user are deleted; an administrator can name another user. The consumption history is not deleted. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        RequestParam(request => request.UserId, "The Id of the user whose resume points are deleted. Optional; when omitted, the current user is used.");
        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the deletion to. Optional.");

        ResponseParam<SuccessResponse>(response => response.IsSuccess, "Whether the operation succeeded.");

        Response(200, "The resume points were removed, or a JSON error with success set to false.",
            example: new SuccessResponse(true)
        );
    }
}
