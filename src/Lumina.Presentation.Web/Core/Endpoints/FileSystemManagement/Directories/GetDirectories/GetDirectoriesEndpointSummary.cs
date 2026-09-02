#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Directories;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDirectoriesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointSummary : Summary<GetDirectoriesEndpoint, GetDirectoriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointSummary"/> class.
    /// </summary>
    public GetDirectoriesEndpointSummary()
    {
        Summary = "Retrieves the directories of a file system path.";
        Description = "Retrieves the directories of the file system path identified by the request.";

        RequestParam(r => r.Path, "The file system path for which to get the directories. Required.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not. Optional.");

        ExampleRequest = new GetDirectoriesRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: true
        );

        Response(200, "The directories of the file system path are returned.", example: new SuccessResponse<DirectoryDto[]>(true, default));
    }
}
