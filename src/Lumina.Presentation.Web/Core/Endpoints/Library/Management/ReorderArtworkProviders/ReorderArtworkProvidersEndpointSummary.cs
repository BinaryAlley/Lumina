#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.ReorderArtworkProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="ReorderArtworkProvidersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderArtworkProvidersEndpointSummary : Summary<ReorderArtworkProvidersEndpoint, ReorderLibraryArtworkProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderArtworkProvidersEndpointSummary"/> class.
    /// </summary>
    public ReorderArtworkProvidersEndpointSummary()
    {
        Summary = "Reorders the artwork providers of a media library.";
        Description = "Reorders the artwork providers of the media library identified by the request, in the provided order.";
        RequestParam(r => r.LibraryId, "The Id of the media library whose artwork providers are reordered. Required.");

        ExampleRequest = new ReorderLibraryArtworkProvidersRequest
        {
            LibraryId = Guid.NewGuid(),
            PluginIds = [Guid.NewGuid(), Guid.NewGuid()]
        };

        Response(200, "The artwork providers of the media library are reordered.", example: new SuccessResponse(true));
    }
}
