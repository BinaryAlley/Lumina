#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.UsersManagement;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Class used for providing a textual description for the <see cref="CheckInitializationEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class CheckInitializationEndpointSummary : Summary<CheckInitializationEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationEndpointSummary"/> class.
    /// </summary>
    public CheckInitializationEndpointSummary()
    {
        Summary = "Checks the initialization status of the application.";
        Description = "Checks the initialization status of the application (if the Admin account exists).";

        Response<InitializationResponse>(200, "The application initialization status is returned.");
    }
}
