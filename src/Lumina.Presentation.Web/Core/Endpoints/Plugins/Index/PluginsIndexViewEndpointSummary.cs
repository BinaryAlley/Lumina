#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Plugins.Index;

/// <summary>
/// Class used for providing a textual description for the <see cref="PluginsIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginsIndexViewEndpointSummary : Summary<PluginsIndexViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsIndexViewEndpointSummary"/> class.
    /// </summary>
    public PluginsIndexViewEndpointSummary()
    {
        Summary = "Renders the plugins management view.";
        Description = "Renders the view for managing the plugins and their settings.";

        Response(200, "The view for managing the plugins and their settings is rendered.");
    }
}
