#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.ManageRoles;

/// <summary>
/// Class used for providing a textual description for the <see cref="ManageRolesViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ManageRolesViewEndpointSummary : Summary<ManageRolesViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManageRolesViewEndpointSummary"/> class.
    /// </summary>
    public ManageRolesViewEndpointSummary()
    {
        Summary = "Renders the manage roles view.";
        Description = "Renders the view for managing the authorization roles.";

        Response(200, "The view for managing the authorization roles is rendered.");
    }
}
