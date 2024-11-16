#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Domain.Common.Enums.FileSystem;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.FileSystem.GetType;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTypeEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTypeEndpointSummary : Summary<GetTypeEndpoint, FileSystemTypeResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTypeEndpointSummary"/> class.
    /// </summary>
    public GetTypeEndpointSummary()
    {
        Summary = "Gets the type of the file system.";
        Description = "Returns the type of the file system.";

        Response(200, "The file system type is returned.",
            example: new FileSystemTypeResponse(
                PlatformType: PlatformType.Unix
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
                    instance = "/api/v1/file-system/get-type"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/file-system/get-type"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/file-system/get-type"
                }
            }
        );
    }
}
