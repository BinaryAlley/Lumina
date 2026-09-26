#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooksLite;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetBooksLiteEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteEndpointSummary : Summary<GetBooksLiteEndpoint, GetBooksLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteEndpointSummary"/> class.
    /// </summary>
    public GetBooksLiteEndpointSummary()
    {
        Summary = "Retrieves the list of lightweight book details of a media library.";
        Description = "Returns the paginated, filterable list of the lightweight details of the books of the media library identified by the route, suitable for card-style navigation. The page is returned to an Admin, who can see the books of all libraries, or to the owner of the library.";

        ExampleRequest = new GetBooksLiteRequest(
            CurrentPage: 1,
            PerPage: 10,
            SearchTerm: "fellowship",
            FilterAlphaKey: "f",
            ShouldIgnoreThePrefixForAlphaPicker: true,
            SortBy: "title",
            SortOrder: SortOrder.Ascending
        );

        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of books to retrieve per page. Optional.");
        RequestParam(r => r.SearchTerm, "The search term used to filter results. Optional.");
        RequestParam(r => r.FilterAlphaKey, "The alpha key used to filter the results by the first character of their title, for the alpha picker. Optional.");
        RequestParam(r => r.ShouldIgnoreThePrefixForAlphaPicker, "Whether the leading \"The \" prefix of a title should be ignored when computing the alpha key, or not. Required.");
        RequestParam(r => r.SortBy, "The name of the field by which to sort the results. Optional.");
        RequestParam(r => r.SortOrder, "The direction in which to sort the results. Optional.");

        ResponseParam<PaginatedResponse<BookLiteResponse>>(r => r.Data, "The lightweight book details on the current page.");
        ResponseParam<PaginatedResponse<BookLiteResponse>>(r => r.CurrentPage, "The current page number.");
        ResponseParam<PaginatedResponse<BookLiteResponse>>(r => r.PerPage, "The number of books retrieved per page.");
        ResponseParam<PaginatedResponse<BookLiteResponse>>(r => r.Count, "The total number of books matching the request.");
        ResponseParam<PaginatedResponse<BookLiteResponse>>(r => r.NumberOfPages, "The total number of pages.");

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
                    detail = "Authentication failed",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/lite"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/lite",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/books/lite",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "InvalidFilterAlphaKey"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
