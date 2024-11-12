#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.FileSystem.GetType;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTypeEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTypeEndpointSummary : Summary<GetTypeEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTypeEndpointSummary"/> class.
    /// </summary>
    public GetTypeEndpointSummary()
    {
        Summary = "Gets the type of the file system.";
        Description = "Returns the type of the file system.";

        Response<FileSystemTypeResponse>(200, "The file system type is returned.");
    }
}
