#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Mapping configuration for books between the domain object and the persistence object.
/// </summary>
public class BookMappingConfig : IRegister
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Registers the mapping configuration.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Book, BookModel>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Metadata.Title)
            .Map(dest => dest.OriginalTitle, src => src.Metadata.OriginalTitle.HasValue ? src.Metadata.OriginalTitle.Value : default)
            .Map(dest => dest.Description, src => src.Metadata.Description.HasValue ? src.Metadata.Description.Value : default)
            .Map(dest => dest.OriginalReleaseDate, src => src.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? src.Metadata.ReleaseInfo.OriginalReleaseDate.Value : default)
            .Map(dest => dest.OriginalReleaseYear, src => src.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? src.Metadata.ReleaseInfo.OriginalReleaseYear.Value : default)
            .Map(dest => dest.ReReleaseDate, src => src.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? src.Metadata.ReleaseInfo.ReReleaseDate.Value : default)
            .Map(dest => dest.ReReleaseYear, src => src.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? src.Metadata.ReleaseInfo.ReReleaseYear.Value : default)
            .Map(dest => dest.ReleaseCountry, src => src.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? src.Metadata.ReleaseInfo.ReleaseCountry.Value : default)
            .Map(dest => dest.ReleaseVersion, src => src.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? src.Metadata.ReleaseInfo.ReleaseVersion.Value : default)
            .Map(dest => dest.LanguageCode, src => src.Metadata.Language.HasValue ? src.Metadata.Language.Value.LanguageCode : default)
            .Map(dest => dest.LanguageName, src => src.Metadata.Language.HasValue ? src.Metadata.Language.Value.LanguageName : default)
            .Map(dest => dest.LanguageNativeName, src => src.Metadata.Language.HasValue ? src.Metadata.Language.Value.NativeName : default)
            .Map(dest => dest.OriginalLanguageCode, src => src.Metadata.OriginalLanguage.HasValue ? src.Metadata.OriginalLanguage.Value.LanguageCode : default)
            .Map(dest => dest.OriginalLanguageName, src => src.Metadata.OriginalLanguage.HasValue ? src.Metadata.OriginalLanguage.Value.LanguageName : default)
            .Map(dest => dest.OriginalLanguageNativeName, src => src.Metadata.OriginalLanguage.HasValue ? src.Metadata.OriginalLanguage.Value.NativeName : default)
            .Map(dest => dest.Tags, src => src.Metadata.Tags)
            .Map(dest => dest.Genres, src => src.Metadata.Genres)
            .Map(dest => dest.Publisher, src => src.Metadata.Publisher.HasValue ? src.Metadata.Publisher.Value : default)
            .Map(dest => dest.PageCount, src => src.Metadata.PageCount.HasValue ? src.Metadata.PageCount.Value : default)
            .Map(dest => dest.Format, src => src.Format.ToString())
            .Map(dest => dest.Edition, src => src.Edition.HasValue ? src.Edition.Value : default)
            .Map(dest => dest.VolumeNumber, src => src.VolumeNumber.HasValue ? src.VolumeNumber.Value : default)
            .Map(dest => dest.ASIN, src => src.ASIN.HasValue ? src.ASIN.Value : default)
            .Map(dest => dest.GoodreadsId, src => src.GoodreadsId.HasValue ? src.GoodreadsId.Value : default)
            .Map(dest => dest.LCCN, src => src.LCCN.HasValue ? src.LCCN.Value : default)
            .Map(dest => dest.OCLCNumber, src => src.OCLCNumber.HasValue ? src.OCLCNumber.Value : default)
            .Map(dest => dest.OpenLibraryId, src => src.OpenLibraryId.HasValue ? src.OpenLibraryId.Value : default)
            .Map(dest => dest.LibraryThingId, src => src.LibraryThingId.HasValue ? src.LibraryThingId.Value : default)
            .Map(dest => dest.GoogleBooksId, src => src.GoogleBooksId.HasValue ? src.GoogleBooksId.Value : default)
            .Map(dest => dest.BarnesAndNobleId, src => src.BarnesAndNobleId.HasValue ? src.BarnesAndNobleId.Value : default)
            .Map(dest => dest.AppleBooksId, src => src.AppleBooksId.HasValue ? src.AppleBooksId.Value : default)
            .Map(dest => dest.ISBNs, src => src.ISBNs)
            .Map(dest => dest.Ratings, src => src.Ratings)
            .Map(dest => dest.Created, src => src.Created)
            .Map(dest => dest.Updated, src => src.Updated.HasValue ? src.Updated : null);

        config.NewConfig<BookRating, BookRatingModel>()
            .Map(dest => dest.Value, src => src.Value)
            .Map(dest => dest.MaxValue, src => src.MaxValue)
            .Map(dest => dest.VoteCount, src => src.VoteCount.HasValue ? src.VoteCount.Value : default)
            .Map(dest => dest.Source, src => src.Source.HasValue ? src.Source.Value : default);

        config.NewConfig<Optional<BookRatingSource>, BookRatingSource?>()
            .MapWith(src => src.HasValue ? src.Value : default);

        // from BookModel to Book
        config.NewConfig<BookModel, Book>()
        .MapWith(src => Book.Create(
            BookId.Create(src.Id),
            WrittenContentMetadata.Create(
                src.Title,
                Optional<string>.FromNullable(src.OriginalTitle),
                Optional<string>.FromNullable(src.Description),
                ReleaseInfo.Create(
                    Optional<DateOnly>.FromNullable(src.OriginalReleaseDate),
                    Optional<int>.FromNullable(src.OriginalReleaseYear),
                    Optional<DateOnly>.FromNullable(src.ReReleaseDate),
                    Optional<int>.FromNullable(src.ReReleaseYear),
                    Optional<string>.FromNullable(src.ReleaseCountry),
                    Optional<string>.FromNullable(src.ReleaseVersion)
                ).Value,
                src.Genres.Select(g => Genre.Create(g.Name!).Value).ToList(),
                src.Tags.Select(t => Tag.Create(t.Name!).Value).ToList(),
                src.LanguageCode != null ? LanguageInfo.Create(
                    src.LanguageCode,
                    src.LanguageName!,
                    Optional<string>.FromNullable(src.LanguageNativeName)
                ).Value : Optional<LanguageInfo>.None(),
                src.OriginalLanguageCode != null ? LanguageInfo.Create(
                    src.OriginalLanguageCode,
                    src.OriginalLanguageName!,
                    Optional<string>.FromNullable(src.OriginalLanguageNativeName)
                ).Value : Optional<LanguageInfo>.None(),
                Optional<string>.FromNullable(src.Publisher),
                Optional<int>.FromNullable(src.PageCount)
            ).Value,
            Optional<BookFormat>.FromNullable(!string.IsNullOrWhiteSpace(src.Format) ? Enum.Parse<BookFormat>(src.Format) : default),
            Optional<string>.FromNullable(src.Edition),
            Optional<int>.FromNullable(src.VolumeNumber),
            Optional<BookSeries>.None(),
            Optional<string>.FromNullable(src.ASIN),
            Optional<string>.FromNullable(src.GoodreadsId),
            Optional<string>.FromNullable(src.LCCN),
            Optional<string>.FromNullable(src.OCLCNumber),
            Optional<string>.FromNullable(src.OpenLibraryId),
            Optional<string>.FromNullable(src.LibraryThingId),
            Optional<string>.FromNullable(src.GoogleBooksId),
            Optional<string>.FromNullable(src.BarnesAndNobleId),
            Optional<string>.FromNullable(src.AppleBooksId),
            src.Created,
            Optional<DateTime>.FromNullable(src.Updated),
            src.ISBNs.Select(i => Isbn.Create(i.Value!, i.Format ?? default).Value).ToList(),
            new List<MediaContributorId>(),
            src.Ratings.Select(r => BookRating.Create(
                r.Value ?? default,
                r.MaxValue ?? default,
                Optional<BookRatingSource>.FromNullable(r.Source),
                Optional<int>.FromNullable(r.VoteCount)
            ).Value).ToList()
        ).Value);
    }
    #endregion
}