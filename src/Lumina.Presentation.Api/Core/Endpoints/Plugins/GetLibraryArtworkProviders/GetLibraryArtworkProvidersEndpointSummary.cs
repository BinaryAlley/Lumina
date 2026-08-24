#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryArtworkProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryArtworkProvidersEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryArtworkProvidersEndpointSummary : Summary<GetLibraryArtworkProvidersEndpoint, GetLibraryArtworkProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryArtworkProvidersEndpointSummary"/> class.
    /// </summary>
    public GetLibraryArtworkProvidersEndpointSummary()
    {
        Summary = "Retrieves the artwork providers configured for a media library.";
        Description = "Retrieves the artwork providers configured for a media library, with their enabled state and rank.";

        ExampleRequest = new GetLibraryArtworkProvidersRequest(
            LibraryId: Guid.NewGuid()
        );

        RequestParam(r => r.LibraryId, "The Id of the media library whose artwork providers are retrieved. Required.");

        ResponseParam<LibraryArtworkProviderResponse>(r => r.PluginId, "The unique identifier of the plugin providing the artwork.");
        ResponseParam<LibraryArtworkProviderResponse>(r => r.Name, "The display name of the artwork provider.");
        ResponseParam<LibraryArtworkProviderResponse>(r => r.IsEnabled, "Whether the artwork provider is enabled for the media library.");
        ResponseParam<LibraryArtworkProviderResponse>(r => r.Rank, "The rank of the artwork provider, determining the order in which providers are tried.");

        Response(200, "The artwork providers configured for the media library are returned.",
            example: new LibraryArtworkProviderResponse[] {
                new(
                    PluginId: Guid.NewGuid(),
                    Name: "Calibre",
                    IsEnabled: true,
                    Rank: 1
                ),
                new(
                    PluginId: Guid.NewGuid(),
                    Name: "Goodreads",
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
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/{libraryId}/artwork-providers"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/{libraryId}/artwork-providers"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/{libraryId}/artwork-providers"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/libraries/{libraryId}/artwork-providers",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested media library does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "LibraryNotFound",
                instance = "/api/v1/libraries/{libraryId}/artwork-providers",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/libraries/{libraryId}/artwork-providers",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
