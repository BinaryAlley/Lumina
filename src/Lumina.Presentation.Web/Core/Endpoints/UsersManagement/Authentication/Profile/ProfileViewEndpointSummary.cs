#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Profile;

/// <summary>
/// Class used for providing a textual description for the <see cref="ProfileViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ProfileViewEndpointSummary : Summary<ProfileViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewEndpointSummary"/> class.
    /// </summary>
    public ProfileViewEndpointSummary()
    {
        Summary = "Renders the user profile view.";
        Description = "Renders the user profile view of the currently logged in account.";

        Response(200, "The user profile view is rendered.");
    }
}
