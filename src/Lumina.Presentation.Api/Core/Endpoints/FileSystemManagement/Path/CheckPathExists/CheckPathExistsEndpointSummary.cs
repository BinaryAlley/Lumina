#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using FastEndpoints.Swagger;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;

/// <summary>
/// Class used for providing a textual description for the <see cref="CheckPathExistsEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsEndpointSummary : Summary<CheckPathExistsEndpoint, CheckPathExistsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpointSummary"/> class.
    /// </summary>
    public CheckPathExistsEndpointSummary()
    {
        Summary = "Checks the existence of a file system path.";
        Description = "Verifies if the specified file system path exists, with an option to include hidden elements in the search.";

        RequestParam(r => r.Path, "The file system path to check the exitence of.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden elements in the search results, or not.");

        Response<PathSegmentResponse>(200, "Indicates whether the file system path exists.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
