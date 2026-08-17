#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathSeparatorEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathSeparatorEndpointSummary : Summary<GetPathSeparatorEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpointSummary"/> class.
    /// </summary>
    public GetPathSeparatorEndpointSummary()
    {
        Summary = "Retrieves the file system path separator.";
        Description = "Retrieves the path separator of the file system of the machine.";

        Response(200, "The file system path separator is returned.", example: new { success = true, data = new { pathSeparator = "\\" } });
    }
}
