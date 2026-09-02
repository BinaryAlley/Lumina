#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingSectionEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionEndpointSummary : Summary<GetReadingSectionEndpoint, GetReadingSectionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpointSummary"/> class.
    /// </summary>
    public GetReadingSectionEndpointSummary()
    {
        Summary = "Retrieves the content of a reading section of a book.";
        Description = "Retrieves the sanitized HTML content of the reading section of a book, ready to be rendered by the client.";

        ExampleRequest = new GetReadingSectionRequest(
            BookId: Guid.NewGuid(),
            LocationRef: "chapter-1"
        );

        RequestParam(r => r.BookId, "The Id of the book whose reading section is retrieved. Required.");
        RequestParam(r => r.LocationRef, "The opaque location reference of the reading section. Required.");

        ResponseParam<ReadingSectionDto>(r => r.LocationRef, "The opaque location reference of the reading section.");
        ResponseParam<ReadingSectionDto>(r => r.Title, "The title of the reading section, if known.");
        ResponseParam<ReadingSectionDto>(r => r.ContentHtml, "The sanitized HTML content of the reading section.");

        Response(200, "The content of the reading section is returned.",
            example: new ReadingSectionDto(
                LocationRef: "chapter-1",
                Title: "Chapter 1",
                ContentHtml: "<h1>Chapter 1</h1><p>A long journey begins...</p>"
            )
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "You are not authorized",
                instance = "/api/v1/books/{bookId}/reading/sections/{locationRef}"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/books/{bookId}/reading/sections/{locationRef}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the book does not exist, no reader plugin supports its format, the reader plugin is disabled, or the reading section was not found.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "SectionNotFound",
                instance = "/api/v1/books/{bookId}/reading/sections/{locationRef}",
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
                instance = "/api/v1/books/{bookId}/reading/sections/{locationRef}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "BookIdCannotBeEmpty",
                            "LocationRefCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
