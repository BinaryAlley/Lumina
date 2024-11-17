#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThumbnailEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailEndpointSummary : Summary<GetThumbnailEndpoint, GetThumbnailRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpointSummary"/> class.
    /// </summary>
    public GetThumbnailEndpointSummary()
    {
        Summary = "Retrieves a thumbnail for a specified file.";
        Description = "Returns a thumbnail image for the specified file, with the option to specify the quality.";

        ExampleRequest = new GetThumbnailRequest(
            Path: "/media/contributors/s/patrick_stewart.jpg",
            Quality: 75
        );

        RequestParam(r => r.Path, "The path of the file for which to get the thumbnail. Required.");
        RequestParam(r => r.Quality, "The quality to use for the thumbnail.");

        Response(200, "The thumbnail image for the specified file is returned.", contentType: "image/*",
            example: "Binary file content");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/thumbnails/get-thumbnail"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/thumbnails/get-thumbnail"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/thumbnails/get-thumbnail"
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
                instance = "/api/v1/thumbnails/get-thumbnail",
                traceId = "00-9945ec5ab76ade665761d3493dfbafba-7e8bb1005b4ddc50-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/thumbnails/get-thumbnail",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PathCannotBeEmpty",
                            "ImageQaulityMustBeBetweenZeroAndOneHundred"
                        }
                    }
                },
                traceId = "00-839f81e411d7eb91ed5aa91e56b00bbb-7c8bd5dfabdaf2dc-00"
            }
        );
    }
}
