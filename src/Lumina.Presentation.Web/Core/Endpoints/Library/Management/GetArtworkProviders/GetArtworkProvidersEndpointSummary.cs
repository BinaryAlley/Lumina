#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetArtworkProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetArtworkProvidersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtworkProvidersEndpointSummary : Summary<GetArtworkProvidersEndpoint, GetArtworkProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtworkProvidersEndpointSummary"/> class.
    /// </summary>
    public GetArtworkProvidersEndpointSummary()
    {
        Summary = "Retrieves the artwork providers of a media library.";
        Description = "Retrieves the artwork providers of the media library identified by the request.";

        RequestParam(r => r.LibraryId, "The unique identifier of the media library whose artwork providers are retrieved.");

        Response(200, "The artwork providers of the media library are returned.", example: new SuccessResponse<LibraryArtworkProviderDto[]>(true, default));
    }
}
