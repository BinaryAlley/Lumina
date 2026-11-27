#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeArtworkProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryTypeArtworkProvidersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersEndpointSummary : Summary<GetLibraryTypeArtworkProvidersEndpoint, GetLibraryTypeArtworkProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeArtworkProvidersEndpointSummary"/> class.
    /// </summary>
    public GetLibraryTypeArtworkProvidersEndpointSummary()
    {
        Summary = "Retrieves the artwork providers available for a media library type.";
        Description = "Retrieves the artwork providers that support the media library type identified by the route, disabled and ranked in alphabetical order, the way a new media library of that type starts.";
        RequestParam(r => r.LibraryType, "The media library type whose available artwork providers are retrieved. Required.");

        ExampleRequest = new GetLibraryTypeArtworkProvidersRequest(
            LibraryType: "Book"
        );

        Response(200, "The artwork providers available for the media library type are returned.");
    }
}
