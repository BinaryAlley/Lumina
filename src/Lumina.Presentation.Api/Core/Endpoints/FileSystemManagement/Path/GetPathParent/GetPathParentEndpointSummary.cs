#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathParent;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathParentEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetPathParentEndpointSummary : Summary<GetPathParentEndpoint, GetPathParentRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentEndpointSummary"/> class.
    /// </summary>
    public GetPathParentEndpointSummary()
    {
        Summary = "Retrieves the parent directory of the requested file system path.";
        Description = "Fetches the parent directory of a specified file system path, if available.";

        RequestParam(r => r.Path, "The path for which to get the parent directory.");

        Response<IEnumerable<PathSegmentResponse>>(200, "The parent directory of the requested path is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
