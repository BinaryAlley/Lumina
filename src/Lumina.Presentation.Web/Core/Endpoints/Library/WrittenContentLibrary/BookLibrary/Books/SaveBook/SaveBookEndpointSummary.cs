#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;

/// <summary>
/// Class used for providing a textual description for the <see cref="SaveBookEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveBookEndpointSummary : Summary<SaveBookEndpoint, UpdateBookRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveBookEndpointSummary"/> class.
    /// </summary>
    public SaveBookEndpointSummary()
    {
        Summary = "Updates a book.";
        Description = "Updates the details of the book identified by the request.";

        RequestParam(r => r.Id, "The unique identifier of the book to update. Required.");
        RequestParam(r => r.Metadata, "The written content metadata of the book. Required.");
        RequestParam(r => r.Metadata!.Title, "The title of the book. Required.");
        RequestParam(r => r.Metadata!.OriginalTitle, "The original title of the book, if different from the current title. Optional.");
        RequestParam(r => r.Metadata!.Description, "A brief description or summary of the book. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo, "The release information, including release date and other relevant details. Required.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseDate, "The original release date of the book. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.OriginalReleaseYear, "The original release year of the book. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseDate, "The re-release date of the book. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReReleaseYear, "The re-release year of the book. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseCountry, "The country where the book was released. Optional.");
        RequestParam(r => r.Metadata!.ReleaseInfo!.ReleaseVersion, "The version or edition of the release of the book. Optional.");
        RequestParam(r => r.Metadata!.Genres, "The list of genres of the book. Required.");
        RequestParam(r => r.Metadata!.Tags, "The list of tags of the book. Required.");
        RequestParam(r => r.Metadata!.Language, "The language in which the book is written. Optional.");
        RequestParam(r => r.Metadata!.Language!.LanguageCode, "The ISO code of the language of the book. Optional.");
        RequestParam(r => r.Metadata!.Language!.LanguageName, "The name of the language of the book in English. Optional.");
        RequestParam(r => r.Metadata!.Language!.NativeName, "The native name of the language of the book. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage, "The original language of the book, if it has been translated. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageCode, "The ISO code of the original language of the book. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.LanguageName, "The name of the original language of the book in English. Optional.");
        RequestParam(r => r.Metadata!.OriginalLanguage!.NativeName, "The native name of the original language of the book. Optional.");
        RequestParam(r => r.Metadata!.Publisher, "The publisher of the book. Optional.");
        RequestParam(r => r.Metadata!.PageCount, "The number of pages of the book. Optional.");
        RequestParam(r => r.Format, "The format of the book (e.g., Hardcover, Paperback). Optional.");
        RequestParam(r => r.Edition, "The edition of the book. Optional.");
        RequestParam(r => r.VolumeNumber, "The volume or book number in the series. Optional.");
        RequestParam(r => r.Series, "The series the book is part of. Optional.");
        RequestParam(r => r.Series!.Title, "The title of the series the book is part of. Optional.");
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
        RequestParam(r => r.Contributors, "The list of media contributors of the book. Required.");
        RequestParam(r => r.Ratings, "The list of ratings of the book. Required.");

        ExampleRequest = new UpdateBookRequest
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new WrittenContentMetadataDto
            {
                Title = "The Fellowship of the Ring",
                OriginalTitle = "The Fellowship of the Ring",
                Description = "The first part of J.R.R. Tolkien's epic adventure The Lord of the Rings. In a sleepy village in the Shire, young Frodo Baggins finds himself faced with an immense task, as his elderly cousin Bilbo entrusts the Ring to his care. Frodo must leave his home and make a perilous journey across Middle-earth to the Cracks of Doom, there to destroy the Ring and foil the Dark Lord in his evil purpose.",
                ReleaseInfo = new ReleaseInfoDto
                {
                    OriginalReleaseDate = new DateOnly(1954, 7, 29),
                    OriginalReleaseYear = 1954,
                    ReReleaseDate = new DateOnly(2001, 9, 6),
                    ReReleaseYear = 2001,
                    ReleaseCountry = "uk",
                    ReleaseVersion = "50th Anniversary Edition"
                },
                Genres =
                [
                    new GenreDto { Name = "fantasy" },
                    new GenreDto { Name = "adventure" },
                    new GenreDto { Name = "classic" }
                ],
                Tags =
                [
                    new TagDto { Name = "epic fantasy" },
                    new TagDto { Name = "quest" },
                    new TagDto { Name = "middle-earth" }
                ],
                Language = new LanguageInfoDto
                {
                    LanguageCode = "en",
                    LanguageName = "English",
                    NativeName = "English"
                },
                OriginalLanguage = new LanguageInfoDto
                {
                    LanguageCode = "en",
                    LanguageName = "English",
                    NativeName = "English"
                },
                Publisher = "Houghton Mifflin",
                PageCount = 398
            },
            Format = BookFormat.Paperback,
            Edition = "50th Anniversary Edition",
            VolumeNumber = 1,
            Series = new BookSeriesDto
            {
                Title = "The Lord of the Rings"
            },
            ASIN = "B007978NPG",
            GoodreadsId = "3",
            LCCN = "54009621",
            OCLCNumber = "ocm00012345",
            OpenLibraryId = "OL7603910M",
            LibraryThingId = "3203347",
            GoogleBooksId = "aWZzLPhY4o0C",
            BarnesAndNobleId = "1100307790",
            AppleBooksId = "id395211",
            ISBNs =
            [
                new IsbnDto { Value = "0395272238", Format = IsbnFormat.Isbn10 },
                new IsbnDto { Value = "9780395272237", Format = IsbnFormat.Isbn13 }
            ],
            Contributors =
            [
                new MediaContributorDto
                {
                    Name = new MediaContributorNameDto { DisplayName = "J.R.R. Tolkien", LegalName = "John Ronald Reuel Tolkien" },
                    Role = new MediaContributorRoleDto { Name = "author", Category = "Author" }
                },
                new MediaContributorDto
                {
                    Name = new MediaContributorNameDto { DisplayName = "Alan Lee", LegalName = "Alan Lee" },
                    Role = new MediaContributorRoleDto { Name = "illustrator", Category = "Illustrator" }
                }
            ],
            Ratings =
            [
                new BookRatingDto { Source = BookRatingSource.GoogleBooks, Value = 4.36M, MaxValue = 5, VoteCount = 2345678 },
                new BookRatingDto { Source = BookRatingSource.Amazon, Value = 4.7M, MaxValue = 5, VoteCount = 87654 }
            ]
        };

        Response(200, "The updated book is returned.", example: new SuccessResponse<BookDetailsDto>(true, default));
    }
}
