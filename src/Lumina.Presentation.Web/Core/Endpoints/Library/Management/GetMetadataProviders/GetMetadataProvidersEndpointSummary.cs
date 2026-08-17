#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetMetadataProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMetadataProvidersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMetadataProvidersEndpointSummary : Summary<GetMetadataProvidersEndpoint, GetMetadataProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMetadataProvidersEndpointSummary"/> class.
    /// </summary>
    public GetMetadataProvidersEndpointSummary()
    {
        Summary = "Retrieves the metadata providers of a media library.";
        Description = "Retrieves the metadata providers of the media library identified by the request.";

        RequestParam(r => r.LibraryId, "The unique identifier of the media library whose metadata providers are retrieved.");

        Response(200, "The metadata providers of the media library are returned.", example: new SuccessResponse<LibraryMetadataProviderDto[]>(true, default));
    }
}
