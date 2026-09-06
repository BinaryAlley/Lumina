#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateBookCoverEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverEndpointSummary : Summary<UpdateBookCoverEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpointSummary"/> class.
    /// </summary>
    public UpdateBookCoverEndpointSummary()
    {
        Summary = "Updates the cover image of a book.";
        Description = "Updates the cover image of the book identified by the request, with the image uploaded in the multipart form of the request.";

        Response(200, "The relative path of the stored cover image is returned.",
            example: "/media/books/books-3f2504e0-4f89-41d3-9a0c-0305e82c3301/The Lord of the Rings-2b0e5f5a-0b3f-4b7e-8f4a-8c9e3d2f5a6b/cover.jpg"
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "You are not authorized",
                instance = "/api/v1/books/{id}/cover"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/books/{id}/cover",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the book or its media library does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "BookNotFound",
                instance = "/api/v1/books/{id}/cover",
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
                instance = "/api/v1/books/{id}/cover",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "BookIdCannotBeEmpty",
                            "BookCoverCannotBeNull",
                            "FileTooLarge",
                            "CoverFileMustBeAnImage"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
