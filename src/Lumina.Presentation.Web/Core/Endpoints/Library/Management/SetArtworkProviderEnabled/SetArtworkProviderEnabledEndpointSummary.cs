#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetArtworkProviderEnabled;

/// <summary>
/// Class used for providing a textual description for the <see cref="SetArtworkProviderEnabledEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetArtworkProviderEnabledEndpointSummary : Summary<SetArtworkProviderEnabledEndpoint, SetLibraryArtworkProviderEnabledRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetArtworkProviderEnabledEndpointSummary"/> class.
    /// </summary>
    public SetArtworkProviderEnabledEndpointSummary()
    {
        Summary = "Enables or disables an artwork provider of a media library.";
        Description = "Enables or disables the artwork provider of the media library identified by the request.";
        RequestParam(r => r.LibraryId, "The Id of the media library whose artwork provider is enabled or disabled. Required.");
        RequestParam(r => r.PluginId, "The unique identifier of the plugin providing the artwork. Required.");
        RequestParam(r => r.IsEnabled, "Whether the artwork provider should be enabled for the media library, or not. Required.");

        ExampleRequest = new SetLibraryArtworkProviderEnabledRequest
        {
            LibraryId = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            IsEnabled = true
        };

        Response(200, "The artwork provider of the media library is enabled or disabled.", example: new SuccessResponse(true));
    }
}
