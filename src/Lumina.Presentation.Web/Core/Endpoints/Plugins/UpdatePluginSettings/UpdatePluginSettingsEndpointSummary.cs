#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.UpdatePluginSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdatePluginSettingsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsEndpointSummary : Summary<UpdatePluginSettingsEndpoint, UpdatePluginSettingsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpointSummary"/> class.
    /// </summary>
    public UpdatePluginSettingsEndpointSummary()
    {
        Summary = "Updates the settings of a plugin.";
        Description = "Updates the settings of the plugin identified by the request.";
        RequestParam(r => r.PluginId, "The unique identifier of the plugin. Required.");
        RequestParam(r => r.Settings, "The settings of the plugin. Optional.");

        ExampleRequest = new UpdatePluginSettingsRequest
        {
            PluginId = Guid.NewGuid(),
            Settings = new Dictionary<string, string>
            {
                { "preferredLanguage", "fr" },
                { "selectionStrategy", "first" }
            }
        };

        Response(200, "The settings of the plugin are updated.", example: new SuccessResponse(true));
    }
}
