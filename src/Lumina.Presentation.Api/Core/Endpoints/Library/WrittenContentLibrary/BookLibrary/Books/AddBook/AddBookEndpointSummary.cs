#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddBookEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookEndpointSummary : Summary<AddBookEndpoint, AddBookRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookEndpointSummary"/> class.
    /// </summary>
    public AddBookEndpointSummary()
    {
        Summary = "Adds a new book.";
        Description = "Creates a new book and returns its details, including the location of the newly created resource.";

        ExampleRequest = new AddBookRequest(
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
                        Category: MediaContributorRoleCategory.Author
                    )
                ),
                new(
                    Name: new MediaContributorNameDto(
                        DisplayName: "Alan Lee",
                        LegalName: "Alan Lee"
                    ),
                    Role: new MediaContributorRoleDto(
                        Name: "illustrator",
                        Category: MediaContributorRoleCategory.Illustrator
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
            ]
        );

        RequestParam(r => r.LibraryId, "The Id of the media library this book belongs to. Required.");
        RequestParam(r => r.Path, "The file system path of the book. Required.");
        RequestParam(r => r.Metadata, "Written content metadata of the book. Required.");
        RequestParam(r => r.Metadata!.Title, "The title of the written content. Required.");
        RequestParam(r => r.Metadata!.OriginalTitle, "The original title of the written content, if different from the current title. Optional.");
        RequestParam(r => r.Metadata!.Description, "A brief description or summary of the written content. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo, "The release information, including release date and other relevant details. Required.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the content. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the content. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release date of the content. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release year of the content. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseCountry, "The country where the content was released. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the content's release. Optional.");
        RequestParam(r => r.Metadata!.Language, "The language in which the written content is written. Optional.");
        RequestParam(r => r.Metadata!.Language!.LanguageCode, "The ISO code of the language (e.g., \"en\" for English). Required.");
        RequestParam(r => r.Metadata!.Language!.LanguageName, "The name of the language in English. Required.");
        RequestParam(r => r.Metadata!.Language!.NativeName, "The native name of the language (e.g., \"Español\" for Spanish). Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage, "The original language in which the written content is written. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageCode, "The ISO code of the language (e.g., \"en\" for English). Required.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageName, "The name of the language in English. Required.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.NativeName, "The native name of the language (e.g., \"Español\" for Spanish). Optional.");
        RequestParam(r => r.Metadata!.Tags, "The list of tags that further describe or categorize the written content. Required.");
        RequestParam(r => r.Metadata!.Genres, "The list of genres associated with the written content. Required.");
        RequestParam(r => r.Metadata!.Publisher, "The name of the publisher of the written content. Optional.");
        RequestParam(r => r.Metadata!.PageCount, "The number of pages in the written content. Optional.");
        RequestParam(r => r.Format, "The format of the book (e.g., Hardcover, Paperback). Optional.");
        RequestParam(r => r.Edition, "The edition of the book. Optional.");
        RequestParam(r => r.VolumeNumber, "The volume or book number in the series. Optional.");
        RequestParam(r => r.Series, "The series name, if the book is part of a series. Optional.");
        RequestParam(r => r.Series!.Title, "The title of the book series. Required.");
        RequestParam(r => r.ASIN, "The ASIN (Amazon Standard Identification Number) of the book. Optional.");
        RequestParam(r => r.GoodreadsId, "The Goodreads Id of the book. Optional.");
        RequestParam(r => r.LCCN, "The Library of Congress Control Number (LCCN) of the book. Optional.");
        RequestParam(r => r.OCLCNumber, "The OCLC Number (WorldCat identifier) of the book. Optional.");
        RequestParam(r => r.OpenLibraryId, "The Open Library Id of the book. Optional.");
        RequestParam(r => r.LibraryThingId, "The LibraryThing Id of the book. Optional.");
        RequestParam(r => r.GoogleBooksId, "The Google Books Id of the book. Optional.");
        RequestParam(r => r.BarnesAndNobleId, "The Barnes & Noble Id of the book. Optional.");
        RequestParam(r => r.AppleBooksId, "The Apple Books Id of the book. Optional.");
        RequestParam(r => r.ISBNs, "The list of ISBN (International Standard Book Number) of the book. Required.");
        RequestParam(r => r.Contributors, "The list of media contributors (actors, directors, etc) starring in this book. Required.");
        RequestParam(r => r.Ratings, "The list of ratings for this book. Required.");

        ResponseParam<BookResponse>(r => r.Id, "The unique identifier of the book.");
        ResponseParam<BookResponse>(r => r.LibraryId, "The Id of the media library the book belongs to.");
        ResponseParam<BookResponse>(r => r.Path, "The file system path of the book.");
        ResponseParam<BookResponse>(r => r.Metadata, "The written content metadata of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.Title, "The title of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.OriginalTitle, "The original title of the book, if different from the current title.");
        ResponseParam<BookResponse>(r => r.Metadata!.Description, "A brief description or summary of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.ReleaseInfo, "The release information of the book, including its release date and other relevant details.");
        ResponseParam<BookResponse>(r => r.Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release date of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release year of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.ReleaseInfo!.ReleaseCountry, "The country where the book was released.");
        ResponseParam<BookResponse>(r => r.Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.Genres, "The list of genres of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.Tags, "The list of tags of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.Language, "The language in which the book is written.");
        ResponseParam<BookResponse>(r => r.Metadata!.Language!.LanguageCode, "The ISO code of the language of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.Language!.LanguageName, "The name of the language of the book in English.");
        ResponseParam<BookResponse>(r => r.Metadata!.Language!.NativeName, "The native name of the language of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.OriginalLanguage, "The original language of the book, if it has been translated.");
        ResponseParam<BookResponse>(r => r.Metadata!.OriginalLanguage!.LanguageCode, "The ISO code of the original language of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.OriginalLanguage!.LanguageName, "The name of the original language of the book in English.");
        ResponseParam<BookResponse>(r => r.Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.Publisher, "The publisher of the book.");
        ResponseParam<BookResponse>(r => r.Metadata!.PageCount, "The number of pages of the book.");
        ResponseParam<BookResponse>(r => r.Format, "The format of the book (e.g., Hardcover, Paperback), if applicable.");
        ResponseParam<BookResponse>(r => r.Edition, "The edition of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.VolumeNumber, "The volume or book number in the series, if applicable.");
        ResponseParam<BookResponse>(r => r.Series, "The series the book is part of, if applicable.");
        ResponseParam<BookResponse>(r => r.Series!.Title, "The title of the series the book is part of.");
        ResponseParam<BookResponse>(r => r.ASIN, "The ASIN (Amazon Standard Identification Number) of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.GoodreadsId, "The Goodreads ID of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.LCCN, "The Library of Congress Control Number (LCCN) of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.OCLCNumber, "The OCLC Number (WorldCat identifier) of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.OpenLibraryId, "The Open Library ID of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.LibraryThingId, "The LibraryThing ID of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.GoogleBooksId, "The Google Books ID of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.BarnesAndNobleId, "The Barnes & Noble ID of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.AppleBooksId, "The Apple Books ID of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.ISBNs, "The list of ISBN (International Standard Book Number) of the book.");
        ResponseParam<BookResponse>(r => r.Contributors, "The list of media contributors starring in this book.");
        ResponseParam<BookResponse>(r => r.Ratings, "The list of ratings for the book.");
        ResponseParam<BookResponse>(r => r.MetadataStatus, "The status of the metadata enrichment of the book.");
        ResponseParam<BookResponse>(r => r.LastMetadataUpdateUtc, "The date and time when the metadata of the book was last enriched, if applicable.");
        ResponseParam<BookResponse>(r => r.MetadataProvider, "The name of the plugin that enriched the metadata of the book, if applicable.");
        ResponseParam<BookResponse>(r => r.CreatedOnUtc, "The date and time when the book was created.");
        ResponseParam<BookResponse>(r => r.UpdatedOnUtc, "The date and time when the book was last updated, if applicable.");
        ResponseParam<BookResponse>(r => r.CoverPath, "The path of the cover image of the book, if available.");

        Response(201, "The new book is returned.", example:
            new BookResponse(
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
                    Genres:
                    [
                        new(Name: "fantasy"),
                        new(Name: "adventure"),
                        new(Name: "classic")
                    ],
                    Tags:
                    [
                        new(Name: "epic fantasy"),
                        new(Name: "quest"),
                        new(Name: "middle-earth")
                    ],
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
                            Category: MediaContributorRoleCategory.Author
                        )
                    ),
                    new(
                        Name: new MediaContributorNameDto(
                            DisplayName: "Alan Lee",
                            LegalName: "Alan Lee"
                        ),
                        Role: new MediaContributorRoleDto(
                            Name: "illustrator",
                            Category: MediaContributorRoleCategory.Illustrator
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
                MetadataStatus: MetadataStatus.Pending,
                LastMetadataUpdateUtc: default,
                MetadataProvider: default,
                CreatedOnUtc: DateTime.UtcNow,
                UpdatedOnUtc: default,
                CoverPath: null
            )
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

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/books",
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
               instance = "/api/v1/books",
               errors = new Dictionary<string, string[]>
               {
                    {
                        "General.Validation", new[]
                        {
                            "BookLibraryCannotBeNull",
                            "BookPathCannotBeEmpty",
                            "MetadataCannotBeNull",
                            "TitleCannotBeEmpty",
                            "TitleMustBeMaximum255CharactersLong",
                            "OriginalTitleMustBeMaximum255CharactersLong",
                            "DescriptionMustBeMaximum2000CharactersLong",
                            "ReleaseInfoCannotBeNull",
                            "OriginalReleaseYearMustBeBetween1And9999",
                            "ReReleaseYearMustBeBetween1And9999",
                            "CountryCodeMustBe2CharactersLong",
                            "ReleaseVersionMustBeMaximum50CharactersLong",
                            "OriginalReleaseDateAndYearMustMatch",
                            "ReReleaseDateAndYearMustMatch",
                            "ReReleaseYearCannotBeEarlierThanOriginalReleaseYear",
                            "ReReleaseDateCannotBeEarlierThanOriginalReleaseDate",
                            "GenresListCannotBeNull",
                            "GenreNameCannotBeEmpty",
                            "GenreNameMustBeMaximum50CharactersLong",
                            "TagsListCannotBeNull",
                            "TagNameCannotBeEmpty",
                            "TagNameMustBeMaximum50CharactersLong",
                            "LanguageCodeCannotBeEmpty",
                            "LanguageCodeMustBe2CharactersLong",
                            "LanguageNameCannotBeEmpty",
                            "LanguageNameMustBeMaximum50CharactersLong",
                            "LanguageNativeNameMustBeMaximum50CharactersLong",
                            "PublisherMustBeMaximum100CharactersLong",
                            "PageCountMustBeGreaterThanZero",
                            "UnknownBookFormat",
                            "EditionMustBeMaximum50CharactersLong",
                            "VolumeNumberMustBeGreaterThanZero",
                            "AsinMustBe10CharactersLong",
                            "GoodreadsIdMustBeNumeric",
                            "InvalidLccnFormat",
                            "InvalidOclcFormat",
                            "InvalidOpenLibraryId",
                            "LibraryThingIdMustBeMaximum50CharactersLong",
                            "GoogleBooksIdMustBe12CharactersLong",
                            "InvalidGoogleBooksIdFormat",
                            "BarnesAndNoblesIdMustBe10CharactersLong",
                            "InvalidBarnesAndNoblesIdFormat",
                            "InvalidAppleBooksIdFormat",
                            "IsbnListCannotBeNull",
                            "IsbnValueCannotBeEmpty",
                            "InvalidIsbn13Format",
                            "InvalidIsbn10Format",
                            "UnknownIsbnFormat",
                            "ContributorsListCannotBeNull",
                            "ContributorNameCannotBeEmpty",
                            "ContributorDisplayNameCannotBeEmpty",
                            "ContributorDisplayNameMustBeMaximum100CharactersLong",
                            "ContributorLegalNameMustBeMaximum100CharactersLong",
                            "ContributorRoleCannotBeNull",
                            "RoleNameCannotBeEmpty",
                            "RoleNameMustBeMaximum50CharactersLong",
                            "RoleCategoryCannotBeEmpty",
                            "RatingsListCannotBeNull",
                            "RatingValueMustBePositive",
                            "RatingValueCannotBeGreaterThanMaxValue",
                            "RatingMaxValueMustBePositive",
                            "RatingVoteCountMustBePositive"
                        }
                    }
               },
               traceId = "00-839f81e411d7eb91ed5aa91e56b00bbb-7c8bd5dfabdaf2dc-00"
           }
       );
    }
}
