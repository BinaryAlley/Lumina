#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Logout;

/// <summary>
/// Class used for providing a textual description for the <see cref="LogoutEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class LogoutEndpointSummary : Summary<LogoutEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutEndpointSummary"/> class.
    /// </summary>
    public LogoutEndpointSummary()
    {
        Summary = "Logs out the user.";
        Description = "Logs out the currently logged in account and redirects the browser to the login page.";

        Response(200, "The user is logged out, and the browser is redirected to the login page.");
    }
}
