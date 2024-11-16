#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using FastEndpoints.Swagger;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;

/// <summary>
/// Class used for providing a textual description for the <see cref="CheckPathExistsEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsEndpointSummary : Summary<CheckPathExistsEndpoint, CheckPathExistsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpointSummary"/> class.
    /// </summary>
    public CheckPathExistsEndpointSummary()
    {
        Summary = "Checks the existence of a file system path.";
        Description = "Verifies if the specified file system path exists, with an option to include hidden elements in the search.";

        ExampleRequest = new CheckPathExistsRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: true
        );
        RequestExamples.Add(new RequestExample(new CheckPathExistsRequest(
            Path: "/media/movies/",
            IncludeHiddenElements: false
        )));

        RequestParam(r => r.Path, "The file system path to check the exitence of. Required.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden elements in the search results, or not. Optional.");

        Response(200, "Indicates whether the file system path exists.",
            example: new PathExistsResponse(
                Exists: true
            ));

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/path/check-path-exists"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/path/check-path-exists"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/path/check-path-exists"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/path/check-path-exists",
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
