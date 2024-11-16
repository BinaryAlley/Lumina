#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Enums.FileSystem;
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

        ExampleRequest = new GetDirectoryTreeRequest(
            Path: "/media/movies/",
            IncludeFiles: true,
            IncludeHiddenElements: true
        );
        RequestExamples.Add(new RequestExample(new GetDirectoryTreeRequest(
            Path: "/media/movies/",
            IncludeFiles: false,
            IncludeHiddenElements: false
        )));
        RequestExamples.Add(new RequestExample(new GetDirectoryTreeRequest(
            Path: "/media/movies/",
            IncludeFiles: true,
            IncludeHiddenElements: false
        )));
        RequestExamples.Add(new RequestExample(new GetDirectoryTreeRequest(
            Path: "/media/movies/",
            IncludeFiles: false,
            IncludeHiddenElements: true
        )));

        RequestParam(r => r.Path, "The file system path for which to get the directory tree. Required.");
        RequestParam(r => r.IncludeFiles, "Whether to include files along the directories or not. Optional.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not. Optional.");

        Response(200, "The directory tree structure for the specified path.",
            example: new FileSystemTreeNodeResponse[] {
                new() 
                { 
                    Path = "/", 
                    Name = "The Lord of the Rings - The Fellowship of the Ring (2001)", 
                    ItemType = FileSystemItemType.Root, 
                    IsExpanded = true, 
                    ChildrenLoaded = true, 
                    Children = 
                    [
                        new()
                        {
                            Path = "/media/",
                            Name = "media",
                            ItemType = FileSystemItemType.Directory,
                            IsExpanded = true,
                            ChildrenLoaded = true,
                            Children =
                            [
                                new()
                                {
                                    Path = "/media/movies/",
                                    Name = "movies",
                                    ItemType = FileSystemItemType.Directory,
                                    IsExpanded = true,
                                    ChildrenLoaded = true,
                                    Children =
                                    [
                                        new()
                                        {
                                            Path = "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/",
                                            Name = "The Lord of the Rings - The Fellowship of the Ring (2001)",
                                            ItemType = FileSystemItemType.Directory,
                                            IsExpanded = true,
                                            ChildrenLoaded = true,
                                            Children =
                                            [
                                                new()
                                                {
                                                    Path = "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/Extras/",
                                                    Name = "Extras",
                                                    ItemType = FileSystemItemType.Directory,
                                                    IsExpanded = false,
                                                    ChildrenLoaded = false,
                                                    Children = []
                                                },
                                                new()
                                                {
                                                    Path = "/media/movies/The Matrix (1999)/The Matrix.mkv",
                                                    Name = "The Matrix.mkv",
                                                    ItemType = FileSystemItemType.File,
                                                    IsExpanded = false,
                                                    ChildrenLoaded = false,
                                                    Children = []
                                                },
                                                new()
                                                {
                                                    Path = "/media/movies/The Matrix (1999)/The Matrix.en.srt",
                                                    Name = "The Matrix.en.srt",
                                                    ItemType = FileSystemItemType.File,
                                                    IsExpanded = false,
                                                    ChildrenLoaded = false,
                                                    Children = []
                                                }
                                            ]
                                        },
                                    ]
                                },
                            ]
                        },
                    ] 
                },
            });

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/directories/get-directory-tree"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/directories/get-directory-tree"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/directories/get-directory-tree"
                }
            }
        );

        Response(403, "The request failed because the provided path does not exist, or the current user doesn't have permission to access it.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "UnauthorizedAccess",
                instance = "/api/v1/directories/get-directory-tree",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/directories/get-directory-tree",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PathCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-839f81e411d7eb91ed5aa91e56b00bbb-7c8bd5dfabdaf2dc-00"
            }
        );
    }
}
