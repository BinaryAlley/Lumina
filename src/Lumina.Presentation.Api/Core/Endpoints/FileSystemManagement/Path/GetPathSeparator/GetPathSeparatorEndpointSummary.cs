#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathSeparatorEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathSeparatorEndpointSummary : Summary<GetPathSeparatorEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpointSummary"/> class.
    /// </summary>
    public GetPathSeparatorEndpointSummary()
    {
        Summary = "Retrieves the file system path separator character.";
        Description = "Fetches the path separator character for the current file system that the API is running on.";

        Response(200, "The path separator character is returned.",
            example: new PathSeparatorResponse(
                Separator: "/"
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
                    instance = "/api/v1/path/get-path-separator"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/path/get-path-separator"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/path/get-path-separator"
                }
            }
        );
    }
}
