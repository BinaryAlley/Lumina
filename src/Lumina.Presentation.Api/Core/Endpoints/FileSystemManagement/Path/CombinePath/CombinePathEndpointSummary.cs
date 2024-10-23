#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CombinePath;

/// <summary>
/// Class used for providing a textual description for the <see cref="CombinePathEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class CombinePathEndpointSummary : Summary<CombinePathEndpoint, CombinePathRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathEndpointSummary"/> class.
    /// </summary>
    public CombinePathEndpointSummary()
    {
        Summary = "Combines two file system paths.";
        Description = "Combines the specified original path with a new path to form a single file system path.";

        RequestParam(r => r.OriginalPath, "The file system path to combine to.");
        RequestParam(r => r.NewPath, "The file system path to combine with.");

        Response<PathSegmentResponse>(200, "Successfully combined the paths.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
