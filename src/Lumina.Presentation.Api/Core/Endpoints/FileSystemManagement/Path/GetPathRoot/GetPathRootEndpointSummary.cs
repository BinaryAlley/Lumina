#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathRoot;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathRootEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetPathRootEndpointSummary : Summary<GetPathRootEndpoint, GetPathRootRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootEndpointSummary"/> class.
    /// </summary>
    public GetPathRootEndpointSummary()
    {
        Summary = "Retrieves a file system path root segment.";
        Description = "Fetches the root segment of a specified file system path, using the provided path parameter.";
        
        RequestParam(r => r.Path, "The file system path for which to get the root.");
        
        Response<PathSegmentResponse>(200, "The root of the requested path is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
