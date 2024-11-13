#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThumbnailEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailEndpointSummary : Summary<GetThumbnailEndpoint, GetThumbnailRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpointSummary"/> class.
    /// </summary>
    public GetThumbnailEndpointSummary()
    {
        Summary = "Retrieves a thumbnail for a specified file.";
        Description = "Returns a thumbnail image for the specified file, with the option to specify the quality.";

        RequestParam(r => r.Path, "The path of the file for which to get the thumbnail.");
        RequestParam(r => r.Quality, "The quality to use for the thumbnail.");

        Response(200, "The thumbnail image for the specified file is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
