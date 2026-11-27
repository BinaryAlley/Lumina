#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Class used for providing a textual description for the <see cref="InstallPluginEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointSummary : Summary<InstallPluginEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointSummary"/> class.
    /// </summary>
    public InstallPluginEndpointSummary()
    {
        Summary = "Installs a plugin.";
        Description = "Installs the plugin uploaded in the multipart form of the request, placing its assemblies into the plugin storage directory of the API. The plugin is loaded by the API at its next startup.";

        Response(200, "The plugin was successfully installed.", example: new SuccessResponse(true));
    }
}
