#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="ScanLibraryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryEndpointSummary : Summary<ScanLibraryEndpoint, ScanLibraryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryEndpointSummary"/> class.
    /// </summary>
    public ScanLibraryEndpointSummary()
    {
        Summary = "Starts the scan of a media library.";
        Description = "Initiates the scan of the media library identified by the request.";

        RequestParam(r => r.Id, "The unique identifier of the media library to scan. Required.");

        ExampleRequest = new ScanLibraryRequest(
            Id: Guid.NewGuid()
        );

        Response(200, "The scan of the media library is started.", example: new SuccessResponse<ScanLibraryDto>(true, default));
    }
}
