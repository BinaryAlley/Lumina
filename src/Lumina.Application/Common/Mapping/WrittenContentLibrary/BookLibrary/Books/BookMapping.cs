#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="Book"/>.
/// </summary>
public static class BookMapping
{
    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="BookEntity"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static BookEntity ToRepositoryEntity(this Book domainModel)
    {
        return new BookEntity
        {
            Id = domainModel.Id.Value,
            Title = domainModel.Metadata.Title,
            OriginalTitle = domainModel.Metadata.OriginalTitle.HasValue ? domainModel.Metadata.OriginalTitle.Value : null,
            Description = domainModel.Metadata.Description.HasValue ? domainModel.Metadata.Description.Value : null,
            OriginalReleaseDate = domainModel.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? domainModel.Metadata.ReleaseInfo.OriginalReleaseDate.Value : null,
            OriginalReleaseYear = domainModel.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? domainModel.Metadata.ReleaseInfo.OriginalReleaseYear.Value : null,
            ReReleaseDate = domainModel.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? domainModel.Metadata.ReleaseInfo.ReReleaseDate.Value : null,
            ReReleaseYear = domainModel.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? domainModel.Metadata.ReleaseInfo.ReReleaseYear.Value : null,
            ReleaseCountry = domainModel.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? domainModel.Metadata.ReleaseInfo.ReleaseCountry.Value : null,
            ReleaseVersion = domainModel.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? domainModel.Metadata.ReleaseInfo.ReleaseVersion.Value : null,
            LanguageCode = domainModel.Metadata.Language.HasValue ? domainModel.Metadata.Language.Value.LanguageCode : null,
            LanguageName = domainModel.Metadata.Language.HasValue ? domainModel.Metadata.Language.Value.LanguageName : null,
            LanguageNativeName = domainModel.Metadata.Language.HasValue ? domainModel.Metadata.Language.Value.NativeName.Value : null,
            OriginalLanguageCode = domainModel.Metadata.OriginalLanguage.HasValue ? domainModel.Metadata.OriginalLanguage.Value.LanguageCode : null,
            OriginalLanguageName = domainModel.Metadata.OriginalLanguage.HasValue ? domainModel.Metadata.OriginalLanguage.Value.LanguageName : null,
            OriginalLanguageNativeName = domainModel.Metadata.OriginalLanguage.HasValue ? domainModel.Metadata.OriginalLanguage.Value.NativeName.Value : null,
            Tags = domainModel.Metadata.Tags.ToRepositoryEntities().ToHashSet(),
            Genres = domainModel.Metadata.Genres.ToRepositoryEntities().ToHashSet(),
            Publisher = domainModel.Metadata.Publisher.HasValue ? domainModel.Metadata.Publisher.Value : null,
            PageCount = domainModel.Metadata.PageCount.HasValue ? domainModel.Metadata.PageCount.Value : null,
            Format = domainModel.Format.ToString(),
            Edition = domainModel.Edition.HasValue ? domainModel.Edition.Value : null,
            VolumeNumber = domainModel.VolumeNumber.HasValue ? domainModel.VolumeNumber.Value : null,
            ASIN = domainModel.ASIN.HasValue ? domainModel.ASIN.Value : null,
            GoodreadsId = domainModel.GoodreadsId.HasValue ? domainModel.GoodreadsId.Value : null,
            LCCN = domainModel.LCCN.HasValue ? domainModel.LCCN.Value : null,
            OCLCNumber = domainModel.OCLCNumber.HasValue ? domainModel.OCLCNumber.Value : null,
            OpenLibraryId = domainModel.OpenLibraryId.HasValue ? domainModel.OpenLibraryId.Value : null,
            LibraryThingId = domainModel.LibraryThingId.HasValue ? domainModel.LibraryThingId.Value : null,
            GoogleBooksId = domainModel.GoogleBooksId.HasValue ? domainModel.GoogleBooksId.Value : null,
            BarnesAndNobleId = domainModel.BarnesAndNobleId.HasValue ? domainModel.BarnesAndNobleId.Value : null,
            AppleBooksId = domainModel.AppleBooksId.HasValue ? domainModel.AppleBooksId.Value : null,
            ISBNs = domainModel.ISBNs.ToRepositoryEntities().ToList(),
            Ratings = domainModel.Ratings.ToRepositoryEntities().ToList(),
            Created = domainModel.Created,
            Updated = domainModel.Updated.HasValue ? domainModel.Updated : null
        };
    }
}
