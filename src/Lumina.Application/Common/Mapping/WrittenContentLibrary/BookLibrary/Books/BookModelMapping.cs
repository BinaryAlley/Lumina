#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.Entities.Common;
using Lumina.Contracts.Entities.WrittenContentLibrary;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Responses.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="BookEntity"/>.
/// </summary>
public static class BookModelMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository model to be converted.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully converted <see cref="Book"/>, or an error message.
    /// </returns>
    public static ErrorOr<Book> ToDomainModel(this BookEntity repositoryEntity)
    {
        IEnumerable<ErrorOr<Tag>> tagsResult = repositoryEntity.Tags.ToDomainModels();
        foreach (ErrorOr<Tag> tagResult in tagsResult)
            if (tagResult.IsError)
                return tagResult.Errors;

        IEnumerable<ErrorOr<Genre>> genresResult = repositoryEntity.Genres.ToDomainModels();
        foreach (ErrorOr<Genre> genreResult in genresResult)
            if (genreResult.IsError)
                return genreResult.Errors;

        IEnumerable<ErrorOr<Isbn>> isbnsResult = repositoryEntity.ISBNs.ToDomainModels();
        foreach (ErrorOr<Isbn> isbnResult in isbnsResult)
            if (isbnResult.IsError)
                return isbnResult.Errors;

        IEnumerable<ErrorOr<BookRating>> bookRatingsResult = repositoryEntity.Ratings.ToDomainModels();
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

        Optional<BookFormat> bookFormat = Optional<BookFormat>.FromNullable(!string.IsNullOrWhiteSpace(repositoryEntity.Format) ? Enum.Parse<BookFormat>(repositoryEntity.Format) : (BookFormat?)null);
        
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
            repositoryEntity.Created,
            Optional<DateTime>.FromNullable(repositoryEntity.Updated),
            isbnsResult.Select(isbn => isbn.Value).ToList(),
            [],
            bookRatingsResult.Select(bookRating => bookRating.Value).ToList());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository models to be converted.</param>
    /// <returns>The converted domain models.</returns>
    /// <returns>
    /// An colection of <see cref="ErrorOr{TValue}"/> containing either successfully converted <see cref="Book"/>, or error messages.
    /// </returns>
    public static IEnumerable<ErrorOr<Book>> ToDomainModels(this IEnumerable<BookEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToDomainModel());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="BookResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository model to be converted.</param>
    /// <returns>The converted response model.</returns>
    public static BookResponse ToResponse(this BookEntity repositoryEntity)
    {
        ReleaseInfoEntity releaseInfoModel = new(
            repositoryEntity.OriginalReleaseDate,
            repositoryEntity.OriginalReleaseYear,
            repositoryEntity.ReReleaseDate,
            repositoryEntity.ReReleaseYear,
            repositoryEntity.ReleaseCountry,
            repositoryEntity.ReleaseVersion
        );
        // language and original language make sense only if their subproperties have values
        LanguageInfoEntity? languageInfoModel = (!string.IsNullOrWhiteSpace(repositoryEntity.LanguageCode) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.LanguageName) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.LanguageNativeName))
            ? new LanguageInfoEntity(
                repositoryEntity.LanguageCode,
                repositoryEntity.LanguageName,
                repositoryEntity.LanguageNativeName
            ) : null;
        LanguageInfoEntity? originalLanguageInfoModel = (!string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageCode) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageName) ||
                                          !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageNativeName))
            ? new LanguageInfoEntity(
                repositoryEntity.OriginalLanguageCode,
                repositoryEntity.OriginalLanguageName,
                repositoryEntity.OriginalLanguageNativeName
            ) : null;
        WrittenContentMetadataEntity metadata = new(
            repositoryEntity.Title, 
            repositoryEntity.OriginalTitle, 
            repositoryEntity.Description,
            releaseInfoModel,
            [.. repositoryEntity.Genres],
            [.. repositoryEntity.Tags],
            languageInfoModel,
            originalLanguageInfoModel,
            repositoryEntity.Publisher,
            repositoryEntity.PageCount
        );
        return new BookResponse(
            repositoryEntity.Id, 
            metadata,
            !string.IsNullOrWhiteSpace(repositoryEntity.Format) ? Enum.Parse<BookFormat>(repositoryEntity.Format) : null,
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
            repositoryEntity.ISBNs,
            null,
            repositoryEntity.Ratings,
            repositoryEntity.Created,
            repositoryEntity.Updated
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository models to be converted.</param>
    /// <returns>The converted reponses.</returns>
    public static IEnumerable<BookResponse> ToResponses(this IEnumerable<BookEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToResponse());
    }
}