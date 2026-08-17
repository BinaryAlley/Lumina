#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Class used for providing a textual description for the <see cref="RegisterViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterViewEndpointSummary : Summary<RegisterViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewEndpointSummary"/> class.
    /// </summary>
    public RegisterViewEndpointSummary()
    {
        Summary = "Renders the account registration view.";
        Description = "Renders the view for registering a new account.";

        Response(200, "The account registration view is rendered.");
    }
}
