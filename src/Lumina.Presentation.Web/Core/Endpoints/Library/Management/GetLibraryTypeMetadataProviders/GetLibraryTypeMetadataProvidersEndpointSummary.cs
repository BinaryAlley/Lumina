#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeMetadataProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryTypeMetadataProvidersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersEndpointSummary : Summary<GetLibraryTypeMetadataProvidersEndpoint, GetLibraryTypeMetadataProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersEndpointSummary"/> class.
    /// </summary>
    public GetLibraryTypeMetadataProvidersEndpointSummary()
    {
        Summary = "Retrieves the metadata providers available for a media library type.";
        Description = "Retrieves the metadata providers that support the media library type identified by the route, disabled and ranked in alphabetical order, the way a new media library of that type starts.";
        RequestParam(r => r.LibraryType, "The media library type whose available metadata providers are retrieved. Required.");

        ExampleRequest = new GetLibraryTypeMetadataProvidersRequest(
            LibraryType: "Book"
        );

        Response(200, "The metadata providers available for the media library type are returned.");
    }
}
