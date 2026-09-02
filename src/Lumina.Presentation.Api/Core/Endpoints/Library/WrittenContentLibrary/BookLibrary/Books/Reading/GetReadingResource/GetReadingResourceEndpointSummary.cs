#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingResourceEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointSummary : Summary<GetReadingResourceEndpoint, GetReadingResourceRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointSummary"/> class.
    /// </summary>
    public GetReadingResourceEndpointSummary()
    {
        Summary = "Retrieves a resource of a book, for reading.";
        Description = "Retrieves the binary content of a resource of a book, such as an image or a font referenced by a reading section.";

        ExampleRequest = new GetReadingResourceRequest(
            BookId: Guid.NewGuid(),
            ResourceKey: "cover-image"
        );

        RequestParam(r => r.BookId, "The Id of the book whose resource is retrieved. Required.");
        RequestParam(r => r.ResourceKey, "The opaque resource key of the resource. Required.");

        Response(200, "The resource of the book is returned.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "You are not authorized",
                instance = "/api/v1/books/{bookId}/reading/resources/{resourceKey}"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/books/{bookId}/reading/resources/{resourceKey}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the book does not exist, no reader plugin supports its format, the reader plugin is disabled, or the resource was not found.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ResourceNotFound",
                instance = "/api/v1/books/{bookId}/reading/resources/{resourceKey}",
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
                instance = "/api/v1/books/{bookId}/reading/resources/{resourceKey}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "BookIdCannotBeEmpty",
                            "ResourceKeyCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
