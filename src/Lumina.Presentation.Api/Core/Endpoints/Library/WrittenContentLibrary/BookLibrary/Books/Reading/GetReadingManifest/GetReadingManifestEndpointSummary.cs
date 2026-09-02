#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetReadingManifestEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestEndpointSummary : Summary<GetReadingManifestEndpoint, GetReadingManifestRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestEndpointSummary"/> class.
    /// </summary>
    public GetReadingManifestEndpointSummary()
    {
        Summary = "Retrieves the reading manifest of a book.";
        Description = "Retrieves the reading manifest of a book, containing the metadata, the table of contents, the spine, and the resources needed to render the reader.";

        ExampleRequest = new GetReadingManifestRequest(
            BookId: Guid.NewGuid()
        );

        RequestParam(r => r.BookId, "The Id of the book whose reading manifest is retrieved. Required.");

        ResponseParam<ReadingManifestResponse>(r => r.Title, "The title of the book.");
        ResponseParam<ReadingManifestResponse>(r => r.Author, "The author of the book, if known.");
        ResponseParam<ReadingManifestResponse>(r => r.CoverResourceKey, "The resource key of the cover image of the book, if applicable.");
        ResponseParam<ReadingManifestResponse>(r => r.Spine, "The ordered spine of the reading sections of the book.");
        ResponseParam<ReadingManifestResponse>(r => r.ResourceKeys, "The resource keys of the resources of the book.");
        ResponseParam<ReadingManifestResponse>(r => r.HasTextContent, "Whether the book has extractable text content. A scanned book, whose pages are only images, has no text content.");

        Response(200, "The reading manifest of the book is returned.",
            example: new ReadingManifestResponse(
                Title: "The Fellowship of the Ring",
                Author: "J.R.R. Tolkien",
                CoverResourceKey: "cover-image",
                TableOfContents:
                [
                    new ReadingTocEntryResponse(
                        Label: "Chapter 1",
                        LocationRef: "chapter-1",
                        Children: []
                    )
                ],
                Spine:
                [
                    new ReadingSpineItemResponse(
                        LocationRef: "chapter-1",
                        Title: "Chapter 1"
                    )
                ],
                ResourceKeys: ["cover-image"],
                HasTextContent: true
            )
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "You are not authorized",
                instance = "/api/v1/books/{bookId}/reading/manifest"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/books/{bookId}/reading/manifest",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the book does not exist, no reader plugin supports its format, or the reader plugin is disabled.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ReaderDisabled",
                instance = "/api/v1/books/{bookId}/reading/manifest",
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
                instance = "/api/v1/books/{bookId}/reading/manifest",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "BookIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
