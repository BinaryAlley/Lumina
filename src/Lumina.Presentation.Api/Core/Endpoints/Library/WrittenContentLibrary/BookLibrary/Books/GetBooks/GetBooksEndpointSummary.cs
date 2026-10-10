#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetBooksEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksEndpointSummary : Summary<GetBooksEndpoint, GetBooksRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpointSummary"/> class.
    /// </summary>
    public GetBooksEndpointSummary()
    {
        Summary = "Retrieves the list of books of a media library.";
        Description = "Returns the paginated, filterable list of the books of the media library identified by the route, with the full details of each book. The page is returned to an Admin, who can see the books of all libraries, or to the owner of the library.";

        ExampleRequest = new GetBooksRequest(
            CurrentPage: 1,
            PerPage: 10,
            SearchTerm: "fellowship",
            SortBy: "title",
            SortOrder: SortOrder.Ascending
        );

        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of books to retrieve per page. Optional.");
        RequestParam(r => r.SearchTerm, "The search term used to filter results. Optional.");
        RequestParam(r => r.SortBy, "The name of the field by which to sort the results. Optional.");
        RequestParam(r => r.SortOrder, "The direction in which to sort the results. Optional.");

        ResponseParam<PaginatedResponse<BookResponse>>(r => r.Data, "The books on the current page, each with the full details of the book.");
        ResponseParam<PaginatedResponse<BookResponse>>(r => r.CurrentPage, "The current page number.");
        ResponseParam<PaginatedResponse<BookResponse>>(r => r.PerPage, "The number of books retrieved per page.");
        ResponseParam<PaginatedResponse<BookResponse>>(r => r.Count, "The total number of books matching the request.");
        ResponseParam<PaginatedResponse<BookResponse>>(r => r.NumberOfPages, "The total number of pages.");

        Response(200, "The paginated list of books is returned.",
            example: new PaginatedResponse<BookResponse>
            {
                Data = [
                    new(
                        Id: Guid.NewGuid(),
                        LibraryId: Guid.NewGuid(),
                        Path: "/books/the-fellowship-of-the-ring.epub",
                        Metadata: new(
                            Title: "The Fellowship of the Ring",
                            OriginalTitle: "The Fellowship of the Ring",
                            Description: "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings. In a sleepy village in the Shire, young Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care. Frodo must leave his home and make a perilous journey across Middle-earth to the Cracks of Doom, there to destroy the Ring and foil the Dark Lord in his evil purpose.",
                            ReleaseInfo: new(
                                OriginalReleaseDate: DateOnly.ParseExact("1954-07-29", "yyyy-MM-dd", null),
                                OriginalReleaseYear: 1954,
                                ReReleaseDate: DateOnly.ParseExact("2001-09-06", "yyyy-MM-dd", null),
                                ReReleaseYear: 2001,
                                ReleaseCountry: ReleaseCountry.GB,
                                ReleaseVersion: "50th Anniversary Edition"
                            ),
                            Genres: new List<GenreDto>() {
                                { new(Name: "fantasy") },
                                { new(Name: "adventure") },
                                { new(Name: "classic") }
                            },
                            Tags: new List<TagDto>() {
                                { new(Name: "epic fantasy") },
                                { new(Name: "quest") },
                                { new(Name: "middle-earth") }
                            },
                            Language: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            OriginalLanguage: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            Publisher: "Houghton Mifflin",
                            PageCount: 398

                        ),
                        Format: BookFormat.Paperback,
                        Edition: "50th Anniversary Edition",
                        VolumeNumber: 1,
                        Series: null,
                        ASIN: "B007978NPG",
                        GoodreadsId: "3",
                        LCCN: "54009621",
                        OCLCNumber: "ocm00012345",
                        OpenLibraryId: "OL7603910M",
                        LibraryThingId: "3203347",
                        GoogleBooksId: "aWZzLPhY4o0C",
                        BarnesAndNobleId: "1100307790",
                        AppleBooksId: "id395211",
                        ISBNs: [
                            new(
                                Value: "0395272238",
                                Format: IsbnFormat.Isbn10
                            ),
                            new(
                                Value: "9780395272237",
                                Format: IsbnFormat.Isbn13
                            )
                        ],
                        Contributors: [
                            new(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Author
                            ),
                            new(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Illustrator
                            )
                        ],
                        Ratings: [
                            new(
                                Source: BookRatingSource.GoogleBooks,
                                Value: 4.36M,
                                MaxValue: 5,
                                VoteCount: 2345678
                            ),
                            new(
                                Source: BookRatingSource.Amazon,
                                Value: 4.7M,
                                MaxValue: 5,
                                VoteCount: 87654
                            )
                        ],
                        MetadataStatus: MetadataStatus.Pending,
                        LastMetadataUpdateUtc: default,
                        MetadataProvider: default,
                        CreatedOnUtc: DateTime.UtcNow,
                        UpdatedOnUtc: default,
                        CoverPath: null
                    ),
                    new(
                        Id: Guid.NewGuid(),
                        LibraryId: Guid.NewGuid(),
                        Path: "/books/the-two-towers.epub",
                        Metadata: new(
                            Title: "The Two Towers",
                            OriginalTitle: "The Two Towers",
                            Description: "The second part of J.R.R. Tolkien's epic adventure The Lord of the Rings. The Fellowship is scattered, and the quest to destroy the One Ring continues. Aragorn, Legolas, and Gimli search for Merry and Pippin, while Frodo and Sam take the Ring closer to Mordor under the guidance of the mysterious Gollum.",
                            ReleaseInfo: new(
                                OriginalReleaseDate: DateOnly.ParseExact("1954-11-11", "yyyy-MM-dd", null),
                                OriginalReleaseYear: 1954,
                                ReReleaseDate: DateOnly.ParseExact("2001-11-08", "yyyy-MM-dd", null),
                                ReReleaseYear: 2001,
                                ReleaseCountry: ReleaseCountry.GB,
                                ReleaseVersion: "50th Anniversary Edition"
                            ),
                            Genres: new List<GenreDto>() {
                                { new(Name: "fantasy") },
                                { new(Name: "adventure") },
                                { new(Name: "classic") }
                            },
                            Tags: new List<TagDto>() {
                                { new(Name: "epic fantasy") },
                                { new(Name: "quest") },
                                { new(Name: "middle-earth") }
                            },
                            Language: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            OriginalLanguage: new(
                                LanguageCode: "en",
                                LanguageName: "English",
                                NativeName: "English"
                            ),
                            Publisher: "Houghton Mifflin",
                            PageCount: 352
                        ),
                        Format: BookFormat.Hardcover,
                        Edition: "50th Anniversary Edition",
                        VolumeNumber: 2,
                        Series: null,
                        ASIN: "B007978NPZ",
                        GoodreadsId: "33",
                        LCCN: "54009622",
                        OCLCNumber: "ocm00067890",
                        OpenLibraryId: "OL7603911M",
                        LibraryThingId: "3203348",
                        GoogleBooksId: "aWZzLPhY4o1D",
                        BarnesAndNobleId: "1100307791",
                        AppleBooksId: "id395212",
                        ISBNs: [
                            new(
                                Value: "0395272246",
                                Format: IsbnFormat.Isbn10
                            ),
                            new(
                                Value: "9780395272244",
                                Format: IsbnFormat.Isbn13
                            )
                        ],
                        Contributors: [
                            new(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Author
                            ),
                            new(
                                ContributorId: Guid.NewGuid(),
                                Role: MediaContributorRole.Illustrator
                            )
                        ],
                        Ratings: [
                            new(
                                Source: BookRatingSource.GoogleBooks,
                                Value: 4.4M,
                                MaxValue: 5,
                                VoteCount: 2340000
                            ),
                            new(
                                Source: BookRatingSource.Amazon,
                                Value: 4.8M,
                                MaxValue: 5,
                                VoteCount: 90000
                            )
                        ],
                        MetadataStatus: MetadataStatus.Pending,
                        LastMetadataUpdateUtc: default,
                        MetadataProvider: default,
                        CreatedOnUtc: DateTime.UtcNow,
                        UpdatedOnUtc: default,
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/books"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/books",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/books",
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
