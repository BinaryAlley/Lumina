#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetDirectoryTree;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDirectoryTreeEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoryTreeEndpointSummary : Summary<GetDirectoryTreeEndpoint, GetDirectoryTreeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeEndpointSummary"/> class.
    /// </summary>
    public GetDirectoryTreeEndpointSummary()
    {
        Summary = "Retrieves the directory tree structure for a specified file system path.";
        Description = "Returns a hierarchical view of the directory tree, with options to include files and hidden elements.";

        RequestParam(r => r.Path, "The file system path for which to get the directory tree.");
        RequestParam(r => r.IncludeFiles, "Whether to include files along the directories or not.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not.");

        Response<IEnumerable<FileSystemTreeNodeResponse>>(200, "The directory tree structure for the specified path.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
