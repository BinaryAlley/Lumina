#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.SplitPath;

/// <summary>
/// Class used for providing a textual description for the <see cref="SplitPathEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathEndpointSummary : Summary<SplitPathEndpoint, SplitPathRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathEndpointSummary"/> class.
    /// </summary>
    public SplitPathEndpointSummary()
    {
        Summary = "Splits a file system path into its segments.";
        Description = "Splits the file system path identified by the request into its path segments.";

        RequestParam(r => r.Path, "The file system path for which to get the path segments. Required.");

        ExampleRequest = new SplitPathRequest(
            Path: "/media/movies/"
        );

        Response(200, "The path segments of the file system path are returned.", example: new { success = true, data = new { pathSegments = Array.Empty<PathSegmentDto>() } });
    }
}
