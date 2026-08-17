#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.GetPathParent;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathParentEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentEndpointSummary : Summary<GetPathParentEndpoint, GetPathParentRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentEndpointSummary"/> class.
    /// </summary>
    public GetPathParentEndpointSummary()
    {
        Summary = "Retrieves the parent path of a file system path.";
        Description = "Retrieves the path segments of the parent path of the file system path identified by the request.";

        RequestParam(r => r.Path, "The path for which the parent path is retrieved.");

        Response(200, "The path segments of the parent path are returned.", example: new { success = true, data = new { pathSegments = new PathSegmentDto[] { } } });
    }
}
