#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.ReorderMetadataProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="ReorderMetadataProvidersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderMetadataProvidersEndpointSummary : Summary<ReorderMetadataProvidersEndpoint, ReorderLibraryMetadataProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderMetadataProvidersEndpointSummary"/> class.
    /// </summary>
    public ReorderMetadataProvidersEndpointSummary()
    {
        Summary = "Reorders the metadata providers of a media library.";
        Description = "Reorders the metadata providers of the media library identified by the request.";
        RequestParam(r => r.LibraryId, "The Id of the media library whose metadata providers are reordered. Required.");
        RequestParam(r => r.PluginIds, "The plugin Ids in the new order, from highest to lowest rank. Required.");

        ExampleRequest = new ReorderLibraryMetadataProvidersRequest
        {
            LibraryId = Guid.NewGuid(),
            PluginIds = [
                Guid.NewGuid(),
                Guid.NewGuid(),
            ]
        };

        Response(200, "The metadata providers of the media library are reordered.", example: new SuccessResponse(true));
    }
}
