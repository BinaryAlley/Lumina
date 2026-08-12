#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for applying <see cref="BookMetadataDto"/> to <see cref="Book"/>.
/// </summary>
public static class BookMetadataDtoMapping
{
    /// <summary>
    /// Applies the metadata of <paramref name="metadata"/> to <paramref name="book"/>, marking its metadata as enriched by the provided <paramref name="providerName"/>.
    /// </summary>
    /// <param name="book">The domain book to which the metadata is applied.</param>
    /// <param name="metadata">The metadata to be applied.</param>
    /// <param name="providerName">The name of the metadata provider that enriched the book.</param>
    /// <param name="lastUpdateUtc">The date and time when the metadata was enriched.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public static ErrorOr<Success> ApplyMetadata(this Book book, BookMetadataDto metadata, string providerName, DateTime lastUpdateUtc)
    {
        if (string.IsNullOrWhiteSpace(metadata.Title))
            return Domain.SharedKernel.Common.Errors.Errors.Metadata.TitleCannotBeEmpty;
        if (metadata.ReleaseInfo is null)
            return Domain.SharedKernel.Common.Errors.Errors.Metadata.ReleaseInfoCannotBeNull;

        ErrorOr<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(metadata.ReleaseInfo.OriginalReleaseDate),
            Optional<int>.FromNullable(metadata.ReleaseInfo.OriginalReleaseYear),
            Optional<DateOnly>.FromNullable(metadata.ReleaseInfo.ReReleaseDate),
            Optional<int>.FromNullable(metadata.ReleaseInfo.ReReleaseYear),
            Optional<string>.FromNullable(metadata.ReleaseInfo.ReleaseCountry),
            Optional<string>.FromNullable(metadata.ReleaseInfo.ReleaseVersion)
        );
        if (releaseInfoResult.IsError)
            return releaseInfoResult.Errors;

        List<ErrorOr<Genre>> domainGenresResult = metadata.Genres is not null
            ? [.. metadata.Genres.Select(genre => Genre.Create(genre.Name))]
            : [];
        List<Error> genresErrors = [.. domainGenresResult.Where(genreResult => genreResult.IsError).SelectMany(genreResult => genreResult.Errors)];
        if (genresErrors.Count != 0)
            return genresErrors;

        List<ErrorOr<Tag>> domainTagsResult = metadata.Tags is not null
            ? [.. metadata.Tags.Select(tag => Tag.Create(tag.Name))]
            : [];
        List<Error> tagsErrors = [.. domainTagsResult.Where(tagResult => tagResult.IsError).SelectMany(tagResult => tagResult.Errors)];
        if (tagsErrors.Count != 0)
            return tagsErrors;

        Optional<LanguageInfo> languageInfo = Optional<LanguageInfo>.None();
        if (metadata.Language is not null)
        {
            ErrorOr<LanguageInfo> languageInfoResult = LanguageInfo.Create(metadata.Language.LanguageCode, metadata.Language.LanguageName, Optional<string>.FromNullable(metadata.Language.NativeName));
            if (languageInfoResult.IsError)
                return languageInfoResult.Errors;
            languageInfo = languageInfoResult.Value;
        }

        Optional<LanguageInfo> originalLanguageInfo = Optional<LanguageInfo>.None();
        if (metadata.OriginalLanguage is not null)
        {
            ErrorOr<LanguageInfo> originalLanguageInfoResult = LanguageInfo.Create(metadata.OriginalLanguage.LanguageCode, metadata.OriginalLanguage.LanguageName, Optional<string>.FromNullable(metadata.OriginalLanguage.NativeName));
            if (originalLanguageInfoResult.IsError)
                return originalLanguageInfoResult.Errors;
            originalLanguageInfo = originalLanguageInfoResult.Value;
        }

        ErrorOr<WrittenContentMetadata> writtenContentMetadataResult = WrittenContentMetadata.Create(
            metadata.Title,
            Optional<string>.FromNullable(metadata.OriginalTitle),
            Optional<string>.FromNullable(metadata.Description),
            releaseInfoResult.Value,
            [.. domainGenresResult.Select(genreResult => genreResult.Value)],
            [.. domainTagsResult.Select(tagResult => tagResult.Value)],
            languageInfo,
            originalLanguageInfo,
            Optional<string>.FromNullable(metadata.Publisher),
            Optional<int>.FromNullable(metadata.PageCount)
        );
        if (writtenContentMetadataResult.IsError)
            return writtenContentMetadataResult.Errors;

        List<ErrorOr<Isbn>> domainIsbnsResult = metadata.Isbns is not null
            ? [.. metadata.Isbns.Where(isbn => isbn.Value is not null && isbn.Format is not null).Select(isbn => Isbn.Create(isbn.Value, (IsbnFormat)(int)isbn.Format!.Value))]
            : [];
        List<Error> isbnsErrors = [.. domainIsbnsResult.Where(isbnResult => isbnResult.IsError).SelectMany(isbnResult => isbnResult.Errors)];
        if (isbnsErrors.Count != 0)
            return isbnsErrors;

        List<ErrorOr<BookRating>> domainRatingsResult = metadata.Ratings is not null
            ? [.. metadata.Ratings.Where(rating => rating.Value is not null && rating.MaxValue is not null).Select(rating => BookRating.Create(
                    rating.Value!.Value,
                    rating.MaxValue!.Value,
                    Optional<BookRatingSource>.FromNullable((BookRatingSource?)(rating.Source is null ? null : (BookRatingSource)(int)rating.Source.Value)),
                    Optional<int>.FromNullable(rating.VoteCount)
                ))]
            : [];
        List<Error> ratingsErrors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsError).SelectMany(ratingResult => ratingResult.Errors)];
        if (ratingsErrors.Count != 0)
            return ratingsErrors;

        book.ApplyEnrichedMetadata(
            writtenContentMetadataResult.Value,
            Optional<BookFormat>.FromNullable(metadata.Format.HasValue ? (BookFormat)(int)metadata.Format.Value : (BookFormat?)null),
            Optional<string>.FromNullable(metadata.Edition),
            Optional<int>.FromNullable(metadata.VolumeNumber),
            Optional<BookSeries>.None(),
            Optional<string>.FromNullable(metadata.ASIN),
            Optional<string>.FromNullable(metadata.GoodreadsId),
            Optional<string>.FromNullable(metadata.LCCN),
            Optional<string>.FromNullable(metadata.OCLCNumber),
            Optional<string>.FromNullable(metadata.OpenLibraryId),
            Optional<string>.FromNullable(metadata.LibraryThingId),
            Optional<string>.FromNullable(metadata.GoogleBooksId),
            Optional<string>.FromNullable(metadata.BarnesAndNobleId),
            Optional<string>.FromNullable(metadata.AppleBooksId),
            [.. domainIsbnsResult.Select(isbnResult => isbnResult.Value)],
            [.. domainRatingsResult.Select(ratingResult => ratingResult.Value)],
            providerName,
            lastUpdateUtc
        );

        return Result.Success;
    }
}
