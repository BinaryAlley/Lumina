#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.GetPathRoot;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathRootEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootEndpointSummary : Summary<GetPathRootEndpoint, GetPathRootRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootEndpointSummary"/> class.
    /// </summary>
    public GetPathRootEndpointSummary()
    {
        Summary = "Retrieves the root of a file system path.";
        Description = "Retrieves the root of the file system path identified by the request.";

        RequestParam(r => r.Path, "The file system path for which to get the root.");

        Response(200, "The root of the file system path is returned.", example: new { success = true, data = new { root = new PathSegmentDto() } });
    }
}
