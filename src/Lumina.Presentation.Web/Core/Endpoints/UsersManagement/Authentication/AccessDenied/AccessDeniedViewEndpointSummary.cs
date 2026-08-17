#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.AccessDenied;

/// <summary>
/// Class used for providing a textual description for the <see cref="AccessDeniedViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AccessDeniedViewEndpointSummary : Summary<AccessDeniedViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccessDeniedViewEndpointSummary"/> class.
    /// </summary>
    public AccessDeniedViewEndpointSummary()
    {
        Summary = "Renders the access denied view.";
        Description = "Renders the view that is shown when the user is not authorized to access a resource.";

        Response(200, "The access denied view is rendered.");
    }
}
