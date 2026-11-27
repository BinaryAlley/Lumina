#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.ManagePermissions;

/// <summary>
/// Class used for providing a textual description for the <see cref="ManagePermissionsViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ManagePermissionsViewEndpointSummary : Summary<ManagePermissionsViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManagePermissionsViewEndpointSummary"/> class.
    /// </summary>
    public ManagePermissionsViewEndpointSummary()
    {
        Summary = "Renders the manage permissions view.";
        Description = "Renders the view for managing the authorization permissions.";

        Response(200, "The view for managing the authorization permissions is rendered.");
    }
}
