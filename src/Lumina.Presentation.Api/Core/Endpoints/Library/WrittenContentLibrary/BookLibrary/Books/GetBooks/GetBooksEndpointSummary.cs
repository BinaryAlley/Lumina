#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetBooksEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksEndpointSummary : Summary<GetBooksEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpointSummary"/> class.
    /// </summary>
    public GetBooksEndpointSummary()
    {
        Summary = "Retrieves the list of books.";
        Description = "Returns the entire list of books.";


        Response(200, "The list of books is returned.",
            example: new BookResponse[] {
                new(
                    Id: Guid.NewGuid(),
                    Metadata: new(
                        Title: "The Fellowship of the Ring",
                        OriginalTitle: "The Fellowship of the Ring",
                        Description: "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings. In a sleepy village in the Shire, young Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care. Frodo must leave his home and make a perilous journey across Middle-earth to the Cracks of Doom, there to destroy the Ring and foil the Dark Lord in his evil purpose.",
                        ReleaseInfo: new(
                            OriginalReleaseDate: DateOnly.ParseExact("1954-07-29", "yyyy-MM-dd", null),
                            OriginalReleaseYear: 1954,
                            ReReleaseDate: DateOnly.ParseExact("2001-09-06", "yyyy-MM-dd", null),
                            ReReleaseYear: 2001,
                            ReleaseCountry: "uk",
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
                    Series: new BookSeriesDto(
                        Title: "The Lord of the Rings"
                    ),
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
                            Name: new MediaContributorNameDto(
                                DisplayName: "J.R.R. Tolkien",
                                LegalName: "John Ronald Reuel Tolkien"
                            ),
                            Role: new MediaContributorRoleDto(
                                Name: "author",
                                Category: "book"
                            )
                        ),
                        new(
                            Name: new MediaContributorNameDto(
                                DisplayName: "Alan Lee",
                                LegalName: "Alan Lee"
                            ),
                            Role: new MediaContributorRoleDto(
                                Name: "illustrator",
                                Category: "book"
                            )
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
                    CreatedOnUtc: DateTime.UtcNow,
                    UpdatedOnUtc: default
                ),
                new(
                    Id: Guid.NewGuid(),
                    Metadata: new(
                        Title: "The Two Towers",
                        OriginalTitle: "The Two Towers",
                        Description: "The second part of J.R.R. Tolkien's epic adventure The Lord of the Rings. The Fellowship is scattered, and the quest to destroy the One Ring continues. Aragorn, Legolas, and Gimli search for Merry and Pippin, while Frodo and Sam take the Ring closer to Mordor under the guidance of the mysterious Gollum.",
                        ReleaseInfo: new(
                            OriginalReleaseDate: DateOnly.ParseExact("1954-11-11", "yyyy-MM-dd", null),
                            OriginalReleaseYear: 1954,
                            ReReleaseDate: DateOnly.ParseExact("2001-11-08", "yyyy-MM-dd", null),
                            ReReleaseYear: 2001,
                            ReleaseCountry: "uk",
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
                    Series: new BookSeriesDto(
                        Title: "The Lord of the Rings"
                    ),
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
                            Name: new MediaContributorNameDto(
                                DisplayName: "J.R.R. Tolkien",
                                LegalName: "John Ronald Reuel Tolkien"
                            ),
                            Role: new MediaContributorRoleDto(
                                Name: "author",
                                Category: "book"
                            )
                        ),
                        new(
                            Name: new MediaContributorNameDto(
                                DisplayName: "Alan Lee",
                                LegalName: "Alan Lee"
                            ),
                            Role: new MediaContributorRoleDto(
                                Name: "illustrator",
                                Category: "book"
                            )
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
                    CreatedOnUtc: DateTime.UtcNow,
                    UpdatedOnUtc: default
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
                    instance = "/api/v1/books"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/books"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/books"
                }
            }
        );

    }
}
