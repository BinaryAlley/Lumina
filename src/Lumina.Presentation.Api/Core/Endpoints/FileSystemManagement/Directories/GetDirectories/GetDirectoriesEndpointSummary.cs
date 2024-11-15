#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDirectoriesEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointSummary : Summary<GetDirectoriesEndpoint, GetDirectoriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointSummary"/> class.
    /// </summary>
    public GetDirectoriesEndpointSummary()
    {
        Summary = "Retrieves the list of directories for a specified file system path.";
        Description = "Returns the directories within the specified path, with an option to include hidden file system elements.";

        ExampleRequest = new GetDirectoriesRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: true
        );
        RequestExamples.Add(new RequestExample(new GetDirectoriesRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: false
        )));

        RequestParam(r => r.Path, "The file system path for which to get the directories. Required.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not. Optional.");

        Response(200, "The list of directories for the specified path is returned.",
            example: new DirectoryResponse[] {
                new(Path: "/media/movies/The Matrix (1999)/", Name: "The Matrix", DateCreated: DateTime.UtcNow, DateModified: DateTime.UtcNow.AddMinutes(-10), Items: []),
                new(Path: "/media/movies/The Lord of the Rings - The Fellowship of the Ring (2001)/", Name: "The Lord of the Rings - The Fellowship of the Ring (2001)", DateCreated: DateTime.UtcNow, DateModified: DateTime.UtcNow.AddMinutes(-10), Items: [])
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
                    instance = "/api/v1/directories/get-directories"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/directories/get-directories"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/directories/get-directories"
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
                instance = "/api/v1/directories/get-directories",
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
                instance = "/api/v1/directories/get-directories",
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
