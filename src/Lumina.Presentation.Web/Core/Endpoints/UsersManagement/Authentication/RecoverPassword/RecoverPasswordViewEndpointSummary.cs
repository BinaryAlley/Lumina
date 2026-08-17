#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="RecoverPasswordViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordViewEndpointSummary : Summary<RecoverPasswordViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordViewEndpointSummary"/> class.
    /// </summary>
    public RecoverPasswordViewEndpointSummary()
    {
        Summary = "Renders the account password recovery view.";
        Description = "Renders the view for recovering the password of the account.";

        Response(200, "The account password recovery view is rendered.");
    }
}
