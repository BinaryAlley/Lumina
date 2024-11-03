#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="Book"/>.
/// </summary>
public static class BookMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="BookEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static BookEntity ToRepositoryEntity(this Book domainEntity)
    {
        return new BookEntity
        {
            Id = domainEntity.Id.Value,
            Title = domainEntity.Metadata.Title,
            OriginalTitle = domainEntity.Metadata.OriginalTitle.HasValue ? domainEntity.Metadata.OriginalTitle.Value : null,
            Description = domainEntity.Metadata.Description.HasValue ? domainEntity.Metadata.Description.Value : null,
            OriginalReleaseDate = domainEntity.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? domainEntity.Metadata.ReleaseInfo.OriginalReleaseDate.Value : null,
            OriginalReleaseYear = domainEntity.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? domainEntity.Metadata.ReleaseInfo.OriginalReleaseYear.Value : null,
            ReReleaseDate = domainEntity.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? domainEntity.Metadata.ReleaseInfo.ReReleaseDate.Value : null,
            ReReleaseYear = domainEntity.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? domainEntity.Metadata.ReleaseInfo.ReReleaseYear.Value : null,
            ReleaseCountry = domainEntity.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? domainEntity.Metadata.ReleaseInfo.ReleaseCountry.Value : null,
            ReleaseVersion = domainEntity.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? domainEntity.Metadata.ReleaseInfo.ReleaseVersion.Value : null,
            LanguageCode = domainEntity.Metadata.Language.HasValue ? domainEntity.Metadata.Language.Value.LanguageCode : null,
            LanguageName = domainEntity.Metadata.Language.HasValue ? domainEntity.Metadata.Language.Value.LanguageName : null,
            LanguageNativeName = domainEntity.Metadata.Language.HasValue ? domainEntity.Metadata.Language.Value.NativeName.Value : null,
            OriginalLanguageCode = domainEntity.Metadata.OriginalLanguage.HasValue ? domainEntity.Metadata.OriginalLanguage.Value.LanguageCode : null,
            OriginalLanguageName = domainEntity.Metadata.OriginalLanguage.HasValue ? domainEntity.Metadata.OriginalLanguage.Value.LanguageName : null,
            OriginalLanguageNativeName = domainEntity.Metadata.OriginalLanguage.HasValue ? domainEntity.Metadata.OriginalLanguage.Value.NativeName.Value : null,
            Tags = domainEntity.Metadata.Tags.ToRepositoryEntities().ToHashSet(),
            Genres = domainEntity.Metadata.Genres.ToRepositoryEntities().ToHashSet(),
            Publisher = domainEntity.Metadata.Publisher.HasValue ? domainEntity.Metadata.Publisher.Value : null,
            PageCount = domainEntity.Metadata.PageCount.HasValue ? domainEntity.Metadata.PageCount.Value : null,
            Format = domainEntity.Format.HasValue ? domainEntity.Format.Value : null,
            Edition = domainEntity.Edition.HasValue ? domainEntity.Edition.Value : null,
            VolumeNumber = domainEntity.VolumeNumber.HasValue ? domainEntity.VolumeNumber.Value : null,
            ASIN = domainEntity.ASIN.HasValue ? domainEntity.ASIN.Value : null,
            GoodreadsId = domainEntity.GoodreadsId.HasValue ? domainEntity.GoodreadsId.Value : null,
            LCCN = domainEntity.LCCN.HasValue ? domainEntity.LCCN.Value : null,
            OCLCNumber = domainEntity.OCLCNumber.HasValue ? domainEntity.OCLCNumber.Value : null,
            OpenLibraryId = domainEntity.OpenLibraryId.HasValue ? domainEntity.OpenLibraryId.Value : null,
            LibraryThingId = domainEntity.LibraryThingId.HasValue ? domainEntity.LibraryThingId.Value : null,
            GoogleBooksId = domainEntity.GoogleBooksId.HasValue ? domainEntity.GoogleBooksId.Value : null,
            BarnesAndNobleId = domainEntity.BarnesAndNobleId.HasValue ? domainEntity.BarnesAndNobleId.Value : null,
            AppleBooksId = domainEntity.AppleBooksId.HasValue ? domainEntity.AppleBooksId.Value : null,
            ISBNs = domainEntity.ISBNs.ToRepositoryEntities().ToList(),
            Ratings = domainEntity.Ratings.ToRepositoryEntities().ToList(),
            Created = domainEntity.Created,
            Updated = domainEntity.Updated.HasValue ? domainEntity.Updated : null
        };
    }
}
