#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooksLite;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetBooksLiteEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteEndpointSummary : Summary<GetBooksLiteEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteEndpointSummary"/> class.
    /// </summary>
    public GetBooksLiteEndpointSummary()
    {
        Summary = "Retrieves the lightweight details of the list of books.";
        Description = "Returns a paginated list of the lightweight details of the books of the media library, suitable for card-style navigation.";

        Response(200, "The paginated list of lightweight book details is returned.",
            example: new PaginatedResponse<BookLiteResponse>
            {
                Data = [
                    new(
                        Id: Guid.NewGuid(),
                        Title: "The Fellowship of the Ring",
                        ReleaseYear: 1954,
                        CoverPath: null
                    ),
                    new(
                        Id: Guid.NewGuid(),
                        Title: "The Two Towers",
                        ReleaseYear: 1954,
                        CoverPath: null
                    )
                ],
                CurrentPage = 1,
                PerPage = 10,
                Count = 2,
                NumberOfPages = 1
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
                    instance = "/api/v1/books/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/books/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/books/lite"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/books/lite",
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
