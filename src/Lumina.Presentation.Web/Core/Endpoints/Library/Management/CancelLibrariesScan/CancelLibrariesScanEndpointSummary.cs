#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.CancelLibrariesScan;

/// <summary>
/// Class used for providing a textual description for the <see cref="CancelLibrariesScanEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibrariesScanEndpointSummary : Summary<CancelLibrariesScanEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanEndpointSummary"/> class.
    /// </summary>
    public CancelLibrariesScanEndpointSummary()
    {
        Summary = "Cancels the scan of all the media libraries.";
        Description = "Cancels the running scans of all the media libraries.";

        Response(200, "The scan of all media libraries is cancelled.", example: new SuccessResponse(true));
    }
}
