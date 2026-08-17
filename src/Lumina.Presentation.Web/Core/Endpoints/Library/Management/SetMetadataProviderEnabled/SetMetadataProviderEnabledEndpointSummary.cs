#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetMetadataProviderEnabled;

/// <summary>
/// Class used for providing a textual description for the <see cref="SetMetadataProviderEnabledEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetMetadataProviderEnabledEndpointSummary : Summary<SetMetadataProviderEnabledEndpoint, SetLibraryMetadataProviderEnabledRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetMetadataProviderEnabledEndpointSummary"/> class.
    /// </summary>
    public SetMetadataProviderEnabledEndpointSummary()
    {
        Summary = "Enables or disables a metadata provider of a media library.";
        Description = "Enables or disables the metadata provider of the media library identified by the request.";
        RequestParam(r => r.LibraryId, "The Id of the media library whose metadata provider is enabled or disabled. Required.");
        RequestParam(r => r.PluginId, "The unique identifier of the plugin providing the metadata. Required.");
        RequestParam(r => r.IsEnabled, "Whether the metadata provider should be enabled for the media library, or not. Required.");

        ExampleRequest = new SetLibraryMetadataProviderEnabledRequest
        {
            LibraryId = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            IsEnabled = true
        };

        Response(200, "The metadata provider of the media library is enabled or disabled.", example: new SuccessResponse(true));
    }
}
