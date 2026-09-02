#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryBookReaders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryBookReadersEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersEndpointSummary : Summary<GetLibraryBookReadersEndpoint, GetLibraryBookReadersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryBookReadersEndpointSummary"/> class.
    /// </summary>
    public GetLibraryBookReadersEndpointSummary()
    {
        Summary = "Retrieves the book readers configured for a media library.";
        Description = "Retrieves the book readers configured for a media library, with their supported file extensions and enabled state.";

        ExampleRequest = new GetLibraryBookReadersRequest(
            LibraryId: Guid.NewGuid()
        );

        RequestParam(r => r.LibraryId, "The Id of the media library whose book readers are retrieved. Required.");

        ResponseParam<LibraryBookReaderResponse>(r => r.PluginId, "The unique identifier of the plugin providing the book reader.");
        ResponseParam<LibraryBookReaderResponse>(r => r.Name, "The display name of the book reader.");
        ResponseParam<LibraryBookReaderResponse>(r => r.SupportedExtensions, "The file extensions supported by the book reader.");
        ResponseParam<LibraryBookReaderResponse>(r => r.IsEnabled, "Whether the book reader is enabled for the media library.");

        Response(200, "The book readers configured for the media library are returned.",
            example: new LibraryBookReaderResponse[] {
                new(
                    PluginId: Guid.NewGuid(),
                    Name: "EPUB Reader",
                    SupportedExtensions: [".epub"],
                    IsEnabled: true
                ),
                new(
                    PluginId: Guid.NewGuid(),
                    Name: "PDF Reader",
                    SupportedExtensions: [".pdf"],
                    IsEnabled: false
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
                    instance = "/api/v1/libraries/{libraryId}/book-readers"
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
                instance = "/api/v1/libraries/{libraryId}/book-readers",
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
                instance = "/api/v1/libraries/{libraryId}/book-readers",
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
                instance = "/api/v1/libraries/{libraryId}/book-readers",
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
