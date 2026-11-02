#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeBookReaders;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryTypeBookReadersEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersEndpointSummary : Summary<GetLibraryTypeBookReadersEndpoint, GetLibraryTypeBookReadersRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersEndpointSummary"/> class.
    /// </summary>
    public GetLibraryTypeBookReadersEndpointSummary()
    {
        Summary = "Retrieves the book readers available for a media library type.";
        Description = "Retrieves the book readers of the loaded plugins that support a media library type, with their supported file extensions, in the state a new media library of that type starts with: disabled, in alphabetical order.";

        ExampleRequest = new GetLibraryTypeBookReadersRequest(
            LibraryType: "Book"
        );

        RequestParam(r => r.LibraryType, "The media library type whose available book readers are retrieved. Required.");

        ResponseParam<LibraryBookReaderResponse>(r => r.PluginId, "The unique identifier of the plugin providing the book reader.");
        ResponseParam<LibraryBookReaderResponse>(r => r.Name, "The display name of the book reader.");
        ResponseParam<LibraryBookReaderResponse>(r => r.SupportedExtensions, "The file extensions supported by the book reader.");
        ResponseParam<LibraryBookReaderResponse>(r => r.IsEnabled, "Whether the book reader is enabled. Always false, since no media library is involved.");

        Response(200, "The book readers available for the media library type are returned.",
            example: new LibraryBookReaderResponse[] {
                new(
                    PluginId: Guid.NewGuid(),
                    Name: "EPUB Reader",
                    SupportedExtensions: [".epub"],
                    IsEnabled: false
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
                    detail = "Authentication failed",
                    instance = "/api/v1/libraries/book-readers/{libraryType}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/libraries/book-readers/{libraryType}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/book-readers/{libraryType}"
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
                instance = "/api/v1/libraries/book-readers/{libraryType}",
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
                instance = "/api/v1/libraries/book-readers/{libraryType}",
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
