#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Files.GetFiles;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetFilesEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetFilesEndpointSummary : Summary<GetFilesEndpoint, GetFilesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpointSummary"/> class.
    /// </summary>
    public GetFilesEndpointSummary()
    {
        Summary = "Retrieves the list of files for a specified file system path.";
        Description = "Returns the files within the specified path, with an option to include hidden file system elements.";

        RequestParam(r => r.Path, "The file system path for which to get the files.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not.");

        Response<IEnumerable<FileSystemTreeNodeResponse>>(200, "The list of files is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
