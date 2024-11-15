#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CombinePath;

/// <summary>
/// Class used for providing a textual description for the <see cref="CombinePathEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathEndpointSummary : Summary<CombinePathEndpoint, CombinePathRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathEndpointSummary"/> class.
    /// </summary>
    public CombinePathEndpointSummary()
    {
        Summary = "Combines two file system paths.";
        Description = "Combines the specified original path with a new path to form a single file system path.";

        ExampleRequest = new CombinePathRequest(
            OriginalPath: "/media",
            NewPath: "movies"
        );

        RequestParam(r => r.OriginalPath, "The file system path to combine to. Required.");
        RequestParam(r => r.NewPath, "The file system path to combine with. Required.");

        Response(200, "The combined paths are returned.",
            example: new PathSegmentResponse(
                Path: "/media/movies/"
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
                    instance = "/api/v1/path/combine"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/path/combine"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/path/combine"
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
                instance = "/api/v1/path/combine",
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
