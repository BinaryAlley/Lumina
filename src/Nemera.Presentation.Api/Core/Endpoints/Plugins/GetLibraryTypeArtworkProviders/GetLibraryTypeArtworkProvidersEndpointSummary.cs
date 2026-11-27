#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeArtworkProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryTypeArtworkProvidersEndpoint"/> API endpoint, for OpenAPI.
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
        Description = "Retrieves the artwork providers of the loaded plugins that support a media library type, in the state a new media library of that type starts with: disabled, and ranked in alphabetical order.";

        ExampleRequest = new GetLibraryTypeArtworkProvidersRequest(
            LibraryType: "Book"
        );

        RequestParam(r => r.LibraryType, "The media library type whose available artwork providers are retrieved. Required.");

        ResponseParam<LibraryArtworkProviderResponse>(r => r.PluginId, "The unique identifier of the plugin providing the artwork.");
        ResponseParam<LibraryArtworkProviderResponse>(r => r.Name, "The display name of the artwork provider.");
        ResponseParam<LibraryArtworkProviderResponse>(r => r.IsEnabled, "Whether the artwork provider is enabled. Always false, since no media library is involved.");
        ResponseParam<LibraryArtworkProviderResponse>(r => r.Rank, "The rank of the artwork provider, determining the order in which providers are tried.");

        Response(200, "The artwork providers available for the media library type are returned.",
            example: new LibraryArtworkProviderResponse[] {
                new(
                    PluginId: Guid.NewGuid(),
                    Name: "Goodreads",
                    IsEnabled: false,
                    Rank: 1
                ),
                new(
                    PluginId: Guid.NewGuid(),
                    Name: "OpenLibrary",
                    IsEnabled: false,
                    Rank: 2
                )
            }
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/libraries/artwork-providers/{libraryType}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/libraries/artwork-providers/{libraryType}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/artwork-providers/{libraryType}"
                }
            }
        );

        Response(403, "The request failed because the request carries no user identity.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/libraries/artwork-providers/{libraryType}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/libraries/artwork-providers/{libraryType}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryTypeCannotBeNull",
                            "UnknownLibraryType"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
