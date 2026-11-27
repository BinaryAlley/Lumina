#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings;

/// <summary>
/// Class used for providing a textual description for the <see cref="SettingsViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SettingsViewEndpointSummary : Summary<SettingsViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewEndpointSummary"/> class.
    /// </summary>
    public SettingsViewEndpointSummary()
    {
        Summary = "Renders the user settings view.";
        Description = "Renders the view for editing the settings of the current user.";

        Response(200, "The view for editing the settings of the current user is rendered.");
    }
}
