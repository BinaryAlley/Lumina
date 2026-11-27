#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPluginSettingsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsEndpointSummary : Summary<GetPluginSettingsEndpoint, GetPluginSettingsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpointSummary"/> class.
    /// </summary>
    public GetPluginSettingsEndpointSummary()
    {
        Summary = "Retrieves the settings of a plugin and their schema.";
        Description = "Retrieves the settings and their schema of the plugin identified by the request.";

        RequestParam(r => r.PluginId, "The unique identifier of the plugin whose settings are retrieved. Required.");

        ExampleRequest = new GetPluginSettingsRequest(
            PluginId: Guid.NewGuid()
        );

        Response(200, "The settings and their schema of the plugin are returned.", example: new SuccessResponse<PluginSettingsDto>(true, default));
    }
}
