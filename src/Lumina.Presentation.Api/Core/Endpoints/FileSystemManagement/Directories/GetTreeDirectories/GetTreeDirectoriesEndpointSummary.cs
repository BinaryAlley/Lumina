#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetTreeDirectories;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTreeDirectoriesEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetTreeDirectoriesEndpointSummary : Summary<GetTreeDirectoriesEndpoint, GetTreeDirectoriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesEndpointSummary"/> class.
    /// </summary>
    public GetTreeDirectoriesEndpointSummary()
    {
        Summary = "Retrieves the directory tree structure for the specified file system path.";
        Description = "Returns the directory tree structure, with an option to include hidden file system elements.";

        RequestParam(r => r.Path, "The file system path for which to get the directory tree.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not.");

        Response<IEnumerable<FileSystemTreeNodeResponse>>(200, "The directory tree structure is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
