#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathRoot;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathRootEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootEndpointSummary : Summary<GetPathRootEndpoint, GetPathRootRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootEndpointSummary"/> class.
    /// </summary>
    public GetPathRootEndpointSummary()
    {
        Summary = "Retrieves a file system path root segment.";
        Description = "Fetches the root segment of a specified file system path, using the provided path parameter.";

        ExampleRequest = new GetPathRootRequest(
            Path: "/media/movies/"
        );

        RequestParam(r => r.Path, "The file system path for which to get the root. Required.");

        Response(200, "The root of the requested path is returned.",
            example: new PathSegmentResponse(
                Path: "/"
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
                    instance = "/api/v1/path/get-path-root"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/path/get-path-root"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/path/get-path-root"
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
                instance = "/api/v1/path/get-path-root",
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
