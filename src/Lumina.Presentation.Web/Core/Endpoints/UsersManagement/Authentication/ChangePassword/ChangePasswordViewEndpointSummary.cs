#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="ChangePasswordViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordViewEndpointSummary : Summary<ChangePasswordViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordViewEndpointSummary"/> class.
    /// </summary>
    public ChangePasswordViewEndpointSummary()
    {
        Summary = "Renders the change password view.";
        Description = "Renders the view for changing the password of the account.";

        Response(200, "The view for changing the account password is rendered.");
    }
}
