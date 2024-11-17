#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathParent;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathParentEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentEndpointSummary : Summary<GetPathParentEndpoint, GetPathParentRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentEndpointSummary"/> class.
    /// </summary>
    public GetPathParentEndpointSummary()
    {
        Summary = "Retrieves the parent directory of the requested file system path.";
        Description = "Fetches the parent directory of a specified file system path, if available.";

        ExampleRequest = new GetPathParentRequest(
            Path: "/media/movies/"
       );

        RequestParam(r => r.Path, "The path for which to get the parent directory. Required.");

        Response(200, "The parent directory of the requested path is returned.",
            example: new PathSegmentResponse[] {
                new("/"),
                new("media")
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
                    instance = "/api/v1/path/get-path-parent"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/path/get-path-parent"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/path/get-path-parent"
                }
            }
        );

        Response(403, "The request failed because the provided path has no parent.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "CannotNavigateUp",
                instance = "/api/v1/path/get-path-parent",
                traceId = "00-612a168d318340949b326a8598161059-310ff12684509b92-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/path/get-path-parent",
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
