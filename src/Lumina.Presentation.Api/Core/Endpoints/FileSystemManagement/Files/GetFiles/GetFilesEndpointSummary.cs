#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Files.GetFiles;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetFilesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesEndpointSummary : Summary<GetFilesEndpoint, GetFilesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpointSummary"/> class.
    /// </summary>
    public GetFilesEndpointSummary()
    {
        Summary = "Retrieves the list of files for a specified file system path.";
        Description = "Returns the files within the specified path, with an option to include hidden file system elements.";

        ExampleRequest = new GetFilesRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: true
        );
        RequestExamples.Add(new RequestExample(new GetFilesRequest(
             Path: "/media/movies/",
            IncludeHiddenElements: false
        )));

        RequestParam(r => r.Path, "The file system path for which to get the files. Required.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden file system elements or not. Optional.");

        Response(200, "The list of files is returned.",
            example: new FileResponse[] {
                new(Path: "/media/movies/The Matrix (1999)/The Matrix.mkv", Name: "The Matrix.mkv", DateCreated: DateTime.UtcNow, DateModified: DateTime.UtcNow.AddMinutes(-10), Size: 754045401),
                new(Path: "/media/movies/The Matrix (1999)/The Matrix.en.srt", Name: "The Matrix.en.srt", DateCreated: DateTime.UtcNow, DateModified: DateTime.UtcNow.AddMinutes(-10), Size: 6897)
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
                    instance = "/api/v1/files/get-files"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/files/get-files"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/files/get-files"
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
                instance = "/api/v1/files/get-files",
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
                instance = "/api/v1/files/get-files",
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
