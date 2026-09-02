#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Files;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Files.GetTreeFiles;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTreeFilesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesEndpointSummary : Summary<GetTreeFilesEndpoint, GetTreeFilesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesEndpointSummary"/> class.
    /// </summary>
    public GetTreeFilesEndpointSummary()
    {
        Summary = "Retrieves the file system tree of a file system path.";
        Description = "Retrieves the file system tree of the file system path identified by the request.";

        RequestParam(r => r.Path, "The file system path for which to get the tree files. Required.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not. Optional.");

        ExampleRequest = new GetTreeFilesRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: true
        );

        Response(200, "The file system tree of the file system path is returned.", example: new SuccessResponse<FileSystemTreeNodeDto[]>(true, default));
    }
}
