#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="BookEntity"/>.
/// </summary>
public static class BookEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly converted <see cref="Book"/>, or an error message.
    /// </returns>
    public static ErrorOr<Book> ToDomainEntity(this BookEntity repositoryEntity)
    {
        IEnumerable<ErrorOr<Tag>> tagsResult = repositoryEntity.Tags.ToDomainEntities();
        foreach (ErrorOr<Tag> tagResult in tagsResult)
            if (tagResult.IsError)
                return tagResult.Errors;

        IEnumerable<ErrorOr<Genre>> genresResult = repositoryEntity.Genres.ToDomainEntities();
        foreach (ErrorOr<Genre> genreResult in genresResult)
            if (genreResult.IsError)
                return genreResult.Errors;

        IEnumerable<ErrorOr<Isbn>> isbnsResult = repositoryEntity.ISBNs.ToDomainEntities();
        foreach (ErrorOr<Isbn> isbnResult in isbnsResult)
            if (isbnResult.IsError)
                return isbnResult.Errors;

        IEnumerable<ErrorOr<BookRating>> bookRatingsResult = repositoryEntity.Ratings.ToDomainEntities();
        foreach (ErrorOr<BookRating> bookRatingResult in bookRatingsResult)
            if (bookRatingResult.IsError)
                return bookRatingResult.Errors;

        ErrorOr<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
                    Optional<DateOnly>.FromNullable(repositoryEntity.OriginalReleaseDate),
                    Optional<int>.FromNullable(repositoryEntity.OriginalReleaseYear),
                    Optional<DateOnly>.FromNullable(repositoryEntity.ReReleaseDate),
                    Optional<int>.FromNullable(repositoryEntity.ReReleaseYear),
                    Optional<string>.FromNullable(repositoryEntity.ReleaseCountry),
                    Optional<string>.FromNullable(repositoryEntity.ReleaseVersion)
                );
        if (releaseInfoResult.IsError)
            return releaseInfoResult.Errors;

        Optional<LanguageInfo> languageInfo = Optional<LanguageInfo>.None();
        if (repositoryEntity.LanguageCode is not null)
        {
            ErrorOr<LanguageInfo> languageInfoResult = LanguageInfo.Create(
                    repositoryEntity.LanguageCode,
                    repositoryEntity.LanguageName,
                    Optional<string>.FromNullable(repositoryEntity.LanguageNativeName)
                );
            if (languageInfoResult.IsError)
                return languageInfoResult.Errors;
            languageInfo = languageInfoResult.Value;
        }

        Optional<LanguageInfo> originalLanguageCode = Optional<LanguageInfo>.None();
        if (repositoryEntity.OriginalLanguageCode is not null)
        {
            ErrorOr<LanguageInfo> originalLanguageInfoResult = LanguageInfo.Create(
                    repositoryEntity.OriginalLanguageCode,
                    repositoryEntity.OriginalLanguageName!,
                    Optional<string>.FromNullable(repositoryEntity.OriginalLanguageNativeName)
                );
            if (originalLanguageInfoResult.IsError)
                return originalLanguageInfoResult.Errors;
            originalLanguageCode = originalLanguageInfoResult.Value;
        }

        ErrorOr<WrittenContentMetadata> writtenContentMetadataResult = WrittenContentMetadata.Create(
                repositoryEntity.Title,
                Optional<string>.FromNullable(repositoryEntity.OriginalTitle),
                Optional<string>.FromNullable(repositoryEntity.Description),
                releaseInfoResult.Value,
                genresResult.Select(genre => genre.Value).ToList(),
                tagsResult.Select(tag => tag.Value).ToList(),
                languageInfo,
                originalLanguageCode,
                Optional<string>.FromNullable(repositoryEntity.Publisher),
                Optional<int>.FromNullable(repositoryEntity.PageCount)
            );
        if (writtenContentMetadataResult.IsError)
            return writtenContentMetadataResult.Errors;

        Optional<BookFormat> bookFormat = Optional<BookFormat>.FromNullable(repositoryEntity.Format);

        return Book.Create(
            BookId.Create(repositoryEntity.Id),
            writtenContentMetadataResult.Value,
            bookFormat,
            Optional<string>.FromNullable(repositoryEntity.Edition),
            Optional<int>.FromNullable(repositoryEntity.VolumeNumber),
            Optional<BookSeries>.None(),
            Optional<string>.FromNullable(repositoryEntity.ASIN),
            Optional<string>.FromNullable(repositoryEntity.GoodreadsId),
            Optional<string>.FromNullable(repositoryEntity.LCCN),
            Optional<string>.FromNullable(repositoryEntity.OCLCNumber),
            Optional<string>.FromNullable(repositoryEntity.OpenLibraryId),
            Optional<string>.FromNullable(repositoryEntity.LibraryThingId),
            Optional<string>.FromNullable(repositoryEntity.GoogleBooksId),
            Optional<string>.FromNullable(repositoryEntity.BarnesAndNobleId),
            Optional<string>.FromNullable(repositoryEntity.AppleBooksId),
            repositoryEntity.CreatedOnUtc,
            Optional<DateTime>.FromNullable(repositoryEntity.UpdatedOnUtc),
            isbnsResult.Select(isbn => isbn.Value).ToList(),
            [],
            bookRatingsResult.Select(bookRating => bookRating.Value).ToList());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    /// <returns>
    /// An colection of <see cref="ErrorOr{TValue}"/> containing either successfuly converted <see cref="Book"/>, or error messages.
    /// </returns>
    public static IEnumerable<ErrorOr<Book>> ToDomainEntities(this IEnumerable<BookEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToDomainEntity());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="BookResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static BookResponse ToResponse(this BookEntity repositoryEntity)
    {
        ReleaseInfoDto releaseInfoEntity = new(
            repositoryEntity.OriginalReleaseDate,
            repositoryEntity.OriginalReleaseYear,
            repositoryEntity.ReReleaseDate,
            repositoryEntity.ReReleaseYear,
            repositoryEntity.ReleaseCountry,
            repositoryEntity.ReleaseVersion
        );
        // language and original language make sense only if their subproperties have values
        LanguageInfoDto? languageInfoEntity = !string.IsNullOrWhiteSpace(repositoryEntity.LanguageCode) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.LanguageName) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.LanguageNativeName)
            ? new LanguageInfoDto(
                repositoryEntity.LanguageCode,
                repositoryEntity.LanguageName,
                repositoryEntity.LanguageNativeName
            ) : null;
        LanguageInfoDto? originalLanguageInfoEntity = !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageCode) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageName) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageNativeName)
            ? new LanguageInfoDto(
                repositoryEntity.OriginalLanguageCode,
                repositoryEntity.OriginalLanguageName,
                repositoryEntity.OriginalLanguageNativeName
            ) : null;
        WrittenContentMetadataDto metadata = new(
            repositoryEntity.Title,
            repositoryEntity.OriginalTitle,
            repositoryEntity.Description,
            releaseInfoEntity,
            [.. repositoryEntity.Genres.ToResponses()],
            [.. repositoryEntity.Tags.ToResponses()],
            languageInfoEntity,
            originalLanguageInfoEntity,
            repositoryEntity.Publisher,
            repositoryEntity.PageCount
        );
        return new BookResponse(
            repositoryEntity.Id,
            metadata,
            repositoryEntity.Format,
            repositoryEntity.Edition,
            repositoryEntity.VolumeNumber,
            null,
            repositoryEntity.ASIN,
            repositoryEntity.GoodreadsId,
            repositoryEntity.LCCN,
            repositoryEntity.OCLCNumber,
            repositoryEntity.OpenLibraryId,
            repositoryEntity.LibraryThingId,
            repositoryEntity.GoogleBooksId,
            repositoryEntity.BarnesAndNobleId,
            repositoryEntity.AppleBooksId,
            [.. repositoryEntity.ISBNs.ToResponses()],
            null,
            [.. repositoryEntity.Ratings.ToResponses()],
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted reponses.</returns>
    public static IEnumerable<BookResponse> ToResponses(this IEnumerable<BookEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToResponse());
    }
}
