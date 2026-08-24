#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.ReorderLibraryArtworkProviders;

/// <summary>
/// Class used for providing a textual description for the <see cref="ReorderLibraryArtworkProvidersEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryArtworkProvidersEndpointSummary : Summary<ReorderLibraryArtworkProvidersEndpoint, ReorderLibraryArtworkProvidersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryArtworkProvidersEndpointSummary"/> class.
    /// </summary>
    public ReorderLibraryArtworkProvidersEndpointSummary()
    {
        Summary = "Reorders the artwork providers of a media library.";
        Description = "Reorders the artwork providers of a media library, determining the order in which the providers are tried during the artwork enrichment of the library scans.";

        ExampleRequest = new ReorderLibraryArtworkProvidersRequest(
            LibraryId: Guid.NewGuid(),
            PluginIds:
            [
                Guid.NewGuid(),
                Guid.NewGuid()
            ]
        );

        RequestParam(r => r.LibraryId, "The Id of the media library whose artwork providers are reordered. Required.");
        RequestParam(r => r.PluginIds, "The plugin Ids in the new order, from highest to lowest rank. Required.");

        Response(200, "The artwork providers of the media library were successfully reordered.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/{libraryId}/artwork-providers/reorder"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/{libraryId}/artwork-providers/reorder"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/{libraryId}/artwork-providers/reorder"
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
                instance = "/api/v1/libraries/{libraryId}/artwork-providers/reorder",
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
                instance = "/api/v1/libraries/{libraryId}/artwork-providers/reorder",
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
                instance = "/api/v1/libraries/{libraryId}/artwork-providers/reorder",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "PluginIdsListCannotBeNull",
                            "PluginIdsListCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
