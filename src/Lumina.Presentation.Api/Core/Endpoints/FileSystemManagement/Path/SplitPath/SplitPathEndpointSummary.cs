#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.SplitPath;

/// <summary>
/// Class used for providing a textual description for the <see cref="SplitPathEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class SplitPathEndpointSummary : Summary<SplitPathEndpoint, SplitPathRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathEndpointSummary"/> class.
    /// </summary>
    public SplitPathEndpointSummary()
    {
        Summary = "Splits a file system path.";
        Description = "Splits a file system path into its constituent segments.";

        RequestParam(r => r.Path, "The file system path for which to get the path segments.");

        Response<IEnumerable<PathSegmentResponse>>(200, "Successfully split the path into segments.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
