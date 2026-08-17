#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;

/// <summary>
/// Class used for providing a textual description for the <see cref="CheckPathExistsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsEndpointSummary : Summary<CheckPathExistsEndpoint, CheckPathExistsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpointSummary"/> class.
    /// </summary>
    public CheckPathExistsEndpointSummary()
    {
        Summary = "Checks the existence of a file system path.";
        Description = "Checks whether the file system path identified by the request exists.";

        RequestParam(r => r.Path, "The file system path to check the existence of.");
        RequestParam(r => r.IncludeHiddenElements, "Whether to include hidden elements in the search results, or not.");

        Response(200, "Whether the file system path exists is returned.", example: new { success = true, data = new { exists = true } });
    }
}
