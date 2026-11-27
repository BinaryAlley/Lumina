#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetDeletedBooks;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDeletedBooksEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedBooksEndpointSummary : Summary<GetDeletedBooksEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedBooksEndpointSummary"/> class.
    /// </summary>
    public GetDeletedBooksEndpointSummary()
    {
        Summary = "Retrieves the deleted books of a media library.";
        Description = "Returns the books of the media library identified by the route that the user deleted and that can still be restored, ordered by the time they were deleted, most recent first. The list is returned to an Admin, who can see the deleted books of all libraries, or to the owner of the library.";

        ResponseParam<DeletedBookResponse>(response => response.Id, "The Id of the deleted book.");
        ResponseParam<DeletedBookResponse>(response => response.LibraryId, "The Id of the media library the deleted book belongs to.");
        ResponseParam<DeletedBookResponse>(response => response.Title, "The title of the deleted book.");
        ResponseParam<DeletedBookResponse>(response => response.OriginalTitle, "The original title of the deleted book, if different from the current title.");
        ResponseParam<DeletedBookResponse>(response => response.Path, "The file system path of the deleted book.");
        ResponseParam<DeletedBookResponse>(response => response.DeletedOnUtc, "The date and time when the book was deleted.");
        ResponseParam<DeletedBookResponse>(response => response.CoverPath, "The path of the cover image of the deleted book, if available.");

        Response(200, "The deleted books are returned.",
            example: new DeletedBookResponse[]
            {
                new(
                    Id: Guid.Parse("0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d"),
                    LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                    Title: "The Fellowship of the Ring",
                    OriginalTitle: "The Fellowship of the Ring",
                    Path: "/media/libraries/books/the-fellowship-of-the-ring.epub",
                    DeletedOnUtc: DateTime.UtcNow,
                    CoverPath: "/media/books/tolkien/the-fellowship-of-the-ring/cover.jpg"
                ),
                new(
                    Id: Guid.Parse("2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e"),
                    LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                    Title: "The Two Towers",
                    OriginalTitle: "The Two Towers",
                    Path: "/media/libraries/books/the-two-towers.epub",
                    DeletedOnUtc: DateTime.UtcNow.AddMinutes(-30),
                    CoverPath: "/media/books/tolkien/the-two-towers/cover.jpg"
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/deleted-items"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/deleted-items"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/deleted-items"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/deleted-items",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/deleted-items",
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
