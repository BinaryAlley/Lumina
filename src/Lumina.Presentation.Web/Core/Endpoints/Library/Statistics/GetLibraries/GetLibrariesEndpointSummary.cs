#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetLibraries;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibrariesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibrariesEndpointSummary : Summary<GetLibrariesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesEndpointSummary"/> class.
    /// </summary>
    public GetLibrariesEndpointSummary()
    {
        Summary = "Gets the enabled libraries available for the library filter.";
        Description = "Proxies the request that gets the enabled media libraries available to the current user to the API server. A regular user receives only their own libraries; an administrator receives every enabled library. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        Response(200, "The enabled media libraries are returned, or a JSON error with success set to false.",
            example: new SuccessResponse<LibraryDto[]>(true, default));
    }
}
