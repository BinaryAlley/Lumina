#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPluginsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsEndpointSummary : Summary<GetPluginsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpointSummary"/> class.
    /// </summary>
    public GetPluginsEndpointSummary()
    {
        Summary = "Retrieves the plugins.";
        Description = "Retrieves the collection of detected plugins.";

        Response(200, "The collection of detected plugins is returned.", example: new SuccessResponse<PluginDto[]>(true, default));
    }
}
