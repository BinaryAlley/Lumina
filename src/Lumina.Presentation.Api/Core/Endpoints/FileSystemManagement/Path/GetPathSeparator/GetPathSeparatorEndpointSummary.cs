#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPathSeparatorEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class GetPathSeparatorEndpointSummary : Summary<GetPathSeparatorEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpointSummary"/> class.
    /// </summary>
    public GetPathSeparatorEndpointSummary()
    {
        Summary = "Retrieves the file system path separator character.";
        Description = "Fetches the path separator character for the current file system that the API is running on.";

        Response<PathSeparatorResponse>(200, "The path separator character is returned.");
    }
}
