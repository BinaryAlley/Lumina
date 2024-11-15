#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Drives.GetDrives;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDrivesEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesEndpointSummary : Summary<GetDrivesEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpointSummary"/> class.
    /// </summary>
    public GetDrivesEndpointSummary()
    {
        Summary = "Retrieves the list of file system drives.";
        Description = "Returns all available drives in the file system, on Windows, or the root path, on UNIX.";


        Response(200, "The list of file system drives is returned.",
            example: new FileSystemTreeNodeResponse[] {
                new() { Path = "C:\\", Name = "C:\\", ItemType = FileSystemItemType.Root, IsExpanded = false, ChildrenLoaded = false, Children = [] },
                new() { Path = "D:\\", Name = "D:\\", ItemType = FileSystemItemType.Root, IsExpanded = false, ChildrenLoaded = false, Children = [] }
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
                    instance = "/api/v1/drives/get-drives"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/drives/get-drives"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/drives/get-drives"
                }
            }
        );
    }
}
