#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetRunningLibraryScans;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetRunningLibraryScansEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRunningLibraryScansEndpointSummary : Summary<GetRunningLibraryScansEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansEndpointSummary"/> class.
    /// </summary>
    public GetRunningLibraryScansEndpointSummary()
    {
        Summary = "Retrieves the running media library scans.";
        Description = "Retrieves the collection of ongoing media library scans.";

        Response(200, "The collection of ongoing media library scans is returned.", example: new SuccessResponse<LibraryScanProgressDto[]>(true, default));
    }
}
