#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.CancelLibraryScan;

/// <summary>
/// Class used for providing a textual description for the <see cref="CancelLibraryScanEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanEndpointSummary : Summary<CancelLibraryScanEndpoint, CancelLibraryScanRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanEndpointSummary"/> class.
    /// </summary>
    public CancelLibraryScanEndpointSummary()
    {
        Summary = "Cancels the scan of a media library.";
        Description = "Cancels a running scan of the media library identified by the request.";

        RequestParam(r => r.LibraryId, "The unique identifier of the media library whose scan is cancelled.");
        RequestParam(r => r.ScanId, "The Id of the scan to cancel.");

        Response(200, "The scan of the media library is cancelled.", example: new SuccessResponse(true));
    }
}
