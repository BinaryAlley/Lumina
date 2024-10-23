#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// Class used for providing a textual description for the <see cref="ValidatePathEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class ValidatePathEndpointSummary : Summary<ValidatePathEndpoint, ValidatePathRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointSummary"/> class.
    /// </summary>
    public ValidatePathEndpointSummary()
    {
        Summary = "Validates a file system path.";
        Description = "Checks if the specified file system path is valid according to predefined criteria.";

        RequestParam(r => r.Path, "The file system path to validate.");

        Response<PathValidResponse>(200, "The requested path is validated.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
