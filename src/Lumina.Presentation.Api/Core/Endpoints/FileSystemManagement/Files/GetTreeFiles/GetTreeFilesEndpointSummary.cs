#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Files.GetTreeFiles;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTreeFilesEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesEndpointSummary : Summary<GetTreeFilesEndpoint, GetTreeFilesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesEndpointSummary"/> class.
    /// </summary>
    public GetTreeFilesEndpointSummary()
    {
        Summary = "Retrieves the tree structure of files for a specified file system path.";
        Description = "Returns the files within the specified path in a hierarchical tree structure, with an option to include hidden file system elements.";

        RequestParam(r => r.Path, "The file system path for which to get the tree files.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not.");

        Response<IEnumerable<FileSystemTreeNodeResponse>>(200, "The tree structure of files is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
