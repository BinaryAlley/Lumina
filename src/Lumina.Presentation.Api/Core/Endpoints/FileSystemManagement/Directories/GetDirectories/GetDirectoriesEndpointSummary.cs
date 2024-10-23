#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDirectoriesEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetDirectoriesEndpointSummary : Summary<GetDirectoriesEndpoint, GetDirectoriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointSummary"/> class.
    /// </summary>
    public GetDirectoriesEndpointSummary()
    {
        Summary = "Retrieves the list of directories for a specified file system path.";
        Description = "Returns the directories within the specified path, with an option to include hidden file system elements.";

        RequestParam(r => r.Path, "The file system path for which to get the directories.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not.");

        Response<IEnumerable<DirectoryResponse>>(200, "The list of directories for the specified path is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
