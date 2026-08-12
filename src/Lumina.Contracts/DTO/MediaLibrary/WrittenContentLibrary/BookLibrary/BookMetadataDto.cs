#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for the metadata of a book returned by a metadata provider.
/// </summary>
/// <param name="Title">The title of the book.</param>
/// <param name="OriginalTitle">The original title of the book, if different from the current title.</param>
/// <param name="Description">A brief description or summary of the book.</param>
/// <param name="ReleaseInfo">The release information, including release date and other relevant details.</param>
/// <param name="Genres">The list of genres associated with the book.</param>
/// <param name="Tags">The list of tags that further describe or categorize the book.</param>
/// <param name="Language">The language in which the book is written.</param>
/// <param name="OriginalLanguage">The original language of the book, if it has been translated.</param>
/// <param name="Publisher">The name of the publisher of the book.</param>
/// <param name="PageCount">The number of pages in the book.</param>
/// <param name="Format">The format of the book (e.g., Hardcover, Paperback), if applicable.</param>
/// <param name="Edition">The edition of the book, if applicable.</param>
/// <param name="VolumeNumber">The volume or book number in the series, if applicable.</param>
/// <param name="Series">The series name, if the book is part of a series.</param>
/// <param name="ASIN">The ASIN (Amazon Standard Identification Number) of the book, if applicable.</param>
/// <param name="GoodreadsId">The Goodreads ID of the book, if applicable.</param>
/// <param name="LCCN">The Library of Congress Control Number (LCCN) of the book, if applicable.</param>
/// <param name="OCLCNumber">The OCLC Number (WorldCat identifier) of the book, if applicable.</param>
/// <param name="OpenLibraryId">The Open Library ID of the book, if applicable.</param>
/// <param name="LibraryThingId">The LibraryThing ID of the book, if applicable.</param>
/// <param name="GoogleBooksId">The Google Books ID of the book, if applicable.</param>
/// <param name="BarnesAndNobleId">The Barnes &amp; Noble ID of the book, if applicable.</param>
/// <param name="AppleBooksId">The Apple Books ID of the book, if applicable.</param>
/// <param name="Isbns">The list of ISBN (International Standard Book Number) of the book.</param>
/// <param name="Contributors">The list of media contributors of the book.</param>
/// <param name="Ratings">The list of ratings for the book.</param>
/// <param name="CoverImageUrl">The URL of the cover image of the book, if applicable.</param>
public sealed record BookMetadataDto(
    string? Title,
    string? OriginalTitle,
    string? Description,
    ReleaseInfoDto? ReleaseInfo,
    List<GenreDto>? Genres,
    List<TagDto>? Tags,
    LanguageInfoDto? Language,
    LanguageInfoDto? OriginalLanguage,
    string? Publisher,
    int? PageCount,
    BookFormat? Format,
    string? Edition,
    int? VolumeNumber,
    BookSeriesDto? Series,
    string? ASIN,
    string? GoodreadsId,
    string? LCCN,
    string? OCLCNumber,
    string? OpenLibraryId,
    string? LibraryThingId,
    string? GoogleBooksId,
    string? BarnesAndNobleId,
    string? AppleBooksId,
    List<IsbnDto>? Isbns,
    List<MediaContributorDto>? Contributors,
    List<BookRatingDto>? Ratings,
    string? CoverImageUrl
) : MetadataDto;
