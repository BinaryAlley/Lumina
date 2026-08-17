#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// Class used for providing a textual description for the <see cref="ScanLibrariesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibrariesEndpointSummary : Summary<ScanLibrariesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpointSummary"/> class.
    /// </summary>
    public ScanLibrariesEndpointSummary()
    {
        Summary = "Starts the scan of all the media libraries.";
        Description = "Initiates the scan of all the media libraries.";

        Response(200, "The scan of all media libraries is started.", example: new SuccessResponse<ScanLibraryDto[]>(true, default));
    }
}
