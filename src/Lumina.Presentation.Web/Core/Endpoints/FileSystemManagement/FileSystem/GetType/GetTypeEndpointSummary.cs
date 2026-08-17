#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.FileSystem.GetType;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTypeEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTypeEndpointSummary : Summary<GetTypeEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTypeEndpointSummary"/> class.
    /// </summary>
    public GetTypeEndpointSummary()
    {
        Summary = "Retrieves the file system platform type.";
        Description = "Retrieves the platform type of the file system of the machine.";

        Response(200, "The type of the file system is returned.", example: new { success = true, data = new { platformType = "Windows" } });
    }
}
