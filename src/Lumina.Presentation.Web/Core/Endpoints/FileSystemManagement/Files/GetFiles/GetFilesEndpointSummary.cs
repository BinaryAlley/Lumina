#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Files;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Files.GetFiles;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetFilesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesEndpointSummary : Summary<GetFilesEndpoint, GetFilesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpointSummary"/> class.
    /// </summary>
    public GetFilesEndpointSummary()
    {
        Summary = "Retrieves the files of a file system path.";
        Description = "Retrieves the files of the file system path identified by the request.";

        RequestParam(r => r.Path, "The file system path for which to get the files. Required.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not. Optional.");

        ExampleRequest = new GetFilesRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: true
        );

        Response(200, "The files of the file system path are returned.", example: new SuccessResponse<FileDto[]>(true, default));
    }
}
