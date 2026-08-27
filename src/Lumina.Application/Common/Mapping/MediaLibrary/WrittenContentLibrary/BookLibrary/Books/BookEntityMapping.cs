#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Book"/>, or an error message.
    /// </returns>
    public static Result<Book> ToDomainEntity(this BookEntity repositoryEntity)
    {
        IEnumerable<Result<Tag>> tagsResult = repositoryEntity.Tags.ToDomainEntities();
        foreach (Result<Tag> tagResult in tagsResult)
            if (tagResult.IsFailure)
                return tagResult.Errors;

        IEnumerable<Result<Genre>> genresResult = repositoryEntity.Genres.ToDomainEntities();
        foreach (Result<Genre> genreResult in genresResult)
            if (genreResult.IsFailure)
                return genreResult.Errors;

        IEnumerable<Result<Isbn>> isbnsResult = repositoryEntity.ISBNs.ToDomainEntities();
        foreach (Result<Isbn> isbnResult in isbnsResult)
            if (isbnResult.IsFailure)
                return isbnResult.Errors;

        IEnumerable<Result<BookRating>> bookRatingsResult = repositoryEntity.Ratings.ToDomainEntities();
        foreach (Result<BookRating> bookRatingResult in bookRatingsResult)
            if (bookRatingResult.IsFailure)
                return bookRatingResult.Errors;

        Result<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
                    Optional<DateOnly>.FromNullable(repositoryEntity.OriginalReleaseDate),
                    Optional<int>.FromNullable(repositoryEntity.OriginalReleaseYear),
                    Optional<DateOnly>.FromNullable(repositoryEntity.ReReleaseDate),
                    Optional<int>.FromNullable(repositoryEntity.ReReleaseYear),
                    Optional<string>.FromNullable(repositoryEntity.ReleaseCountry),
                    Optional<string>.FromNullable(repositoryEntity.ReleaseVersion)
                );
        if (releaseInfoResult.IsFailure)
            return releaseInfoResult.Errors;

        Optional<LanguageInfo> languageInfo = Optional<LanguageInfo>.None();
        if (repositoryEntity.LanguageCode is not null)
        {
            Result<LanguageInfo> languageInfoResult = LanguageInfo.Create(
                    repositoryEntity.LanguageCode,
                    repositoryEntity.LanguageName,
                    Optional<string>.FromNullable(repositoryEntity.LanguageNativeName)
                );
            if (languageInfoResult.IsFailure)
                return languageInfoResult.Errors;
            languageInfo = languageInfoResult.Value;
        }

        Optional<LanguageInfo> originalLanguageCode = Optional<LanguageInfo>.None();
        if (repositoryEntity.OriginalLanguageCode is not null)
        {
            Result<LanguageInfo> originalLanguageInfoResult = LanguageInfo.Create(
                    repositoryEntity.OriginalLanguageCode,
                    repositoryEntity.OriginalLanguageName!,
                    Optional<string>.FromNullable(repositoryEntity.OriginalLanguageNativeName)
                );
            if (originalLanguageInfoResult.IsFailure)
                return originalLanguageInfoResult.Errors;
            originalLanguageCode = originalLanguageInfoResult.Value;
        }

        Result<WrittenContentMetadata> writtenContentMetadataResult = WrittenContentMetadata.Create(
                repositoryEntity.Title,
                Optional<string>.FromNullable(repositoryEntity.OriginalTitle),
                Optional<string>.FromNullable(repositoryEntity.Description),
                releaseInfoResult.Value,
                [.. genresResult.Select(genre => genre.Value)],
                [.. tagsResult.Select(tag => tag.Value)],
                languageInfo,
                originalLanguageCode,
                Optional<string>.FromNullable(repositoryEntity.Publisher),
                Optional<int>.FromNullable(repositoryEntity.PageCount)
            );
        if (writtenContentMetadataResult.IsFailure)
            return writtenContentMetadataResult.Errors;

        Optional<BookFormat> bookFormat = Optional<BookFormat>.FromNullable(repositoryEntity.Format);

        Result<Book> bookResult = Book.Create(
            BookId.Create(repositoryEntity.Id),
            LibraryId.Create(repositoryEntity.LibraryId),
            repositoryEntity.Path,
            writtenContentMetadataResult.Value,
            bookFormat,
            Optional<string>.FromNullable(repositoryEntity.Edition),
            Optional<float>.FromNullable(repositoryEntity.VolumeNumber),
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
            [.. isbnsResult.Select(isbn => isbn.Value)],
            [.. repositoryEntity.BookContributors.Select(bookContributor => MediaContributorId.Create(bookContributor.MediaContributorId))],
            [.. bookRatingsResult.Select(bookRating => bookRating.Value)]);
        if (bookResult.IsFailure)
            return bookResult.Errors;

        return bookResult;
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    /// <returns>
    /// An colection of <see cref="Result{TValue}"/> containing either a collection of converted <see cref="Book"/>, or error messages.
    /// </returns>
    public static IEnumerable<Result<Book>> ToDomainEntities(this IEnumerable<BookEntity> repositoryEntities)
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
            repositoryEntity.LibraryId,
            repositoryEntity.Path,
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
            repositoryEntity.MetadataStatus,
            repositoryEntity.LastMetadataUpdateUtc,
            repositoryEntity.MetadataProvider,
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc,
            repositoryEntity.BookArtwork.FirstOrDefault(artwork => artwork.ArtworkType == ArtworkType.Cover)?.FileName
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Book"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted reponses.</returns>
    public static IReadOnlyList<BookResponse> ToResponses(this IEnumerable<BookEntity> repositoryEntities)
    {
        return [.. repositoryEntities.Select(repositoryEntity => repositoryEntity.ToResponse())];
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a paginated collection of <see cref="BookResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The paginated repository entities to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<BookResponse> ToResponses(this PaginatedResultDto<BookEntity> repositoryEntities)
    {
        return new PaginatedResponse<BookResponse>
        {
            Data = repositoryEntities.Data.ToResponses(),
            CurrentPage = repositoryEntities.CurrentPage,
            PerPage = repositoryEntities.PerPage,
            Count = repositoryEntities.Count,
            NumberOfPages = repositoryEntities.NumberOfPages
        };
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static BookLiteResponse ToLiteResponse(this BookEntity repositoryEntity)
    {
        return new BookLiteResponse(
            repositoryEntity.Id,
            repositoryEntity.Title,
            repositoryEntity.ReReleaseYear ?? repositoryEntity.OriginalReleaseYear,
            repositoryEntity.BookArtwork.FirstOrDefault(artwork => artwork.ArtworkType == ArtworkType.Cover)?.FileName
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted reponses.</returns>
    public static IReadOnlyList<BookLiteResponse> ToLiteResponses(this IEnumerable<BookEntity> repositoryEntities)
    {
        return [.. repositoryEntities.Select(repositoryEntity => repositoryEntity.ToLiteResponse())];
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a paginated collection of <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The paginated repository entities to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<BookLiteResponse> ToLiteResponses(this PaginatedResultDto<BookEntity> repositoryEntities)
    {
        return new PaginatedResponse<BookLiteResponse>
        {
            Data = repositoryEntities.Data.ToLiteResponses(),
            CurrentPage = repositoryEntities.CurrentPage,
            PerPage = repositoryEntities.PerPage,
            Count = repositoryEntities.Count,
            NumberOfPages = repositoryEntities.NumberOfPages
        };
    }
}
