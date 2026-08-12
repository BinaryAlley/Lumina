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
