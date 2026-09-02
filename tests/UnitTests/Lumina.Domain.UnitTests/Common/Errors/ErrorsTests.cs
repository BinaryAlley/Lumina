#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Primitives;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Errors;

/// <summary>
/// Contains unit tests for the static <see cref="Errors"/> catalog classes.
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorsTests
{
    [Fact]
    public void LibraryScanning_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(16, typeof(DomainErrors.LibraryScanning).GetProperties().Length);
        AssertError(DomainErrors.LibraryScanning.LibraryScanIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.CanOnlyStartPendingScans, ErrorType.Forbidden);
        AssertError(DomainErrors.LibraryScanning.CanOnlyCompleteRunningScans, ErrorType.Forbidden);
        AssertError(DomainErrors.LibraryScanning.CanOnlyCancelRunningScans, ErrorType.Forbidden);
        AssertError(DomainErrors.LibraryScanning.CanOnlyFailRunningScans, ErrorType.Forbidden);
        AssertError(DomainErrors.LibraryScanning.ScanIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.LibraryScanAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.LibraryScanning.LibraryAlreadyBeingScanned, ErrorType.Forbidden);
        AssertError(DomainErrors.LibraryScanning.LibraryScanNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.LibraryScanning.TotalScanJobItemsCountMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.TotalScanJobsCountMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.CompletedScanJobItemsCountMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.CompletedScanJobsCountMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.CompletedScanJobItemsCountCantExceedTotalScanJobItemsCount, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.CompletedScanJobsCountCantExceedTotalScanJobsCount, ErrorType.Validation);
        AssertError(DomainErrors.LibraryScanning.ScanJobCurrentOperationCannotBeEmpty, ErrorType.Validation);
    }

    [Fact]
    public void Library_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(14, typeof(DomainErrors.Library).GetProperties().Length);
        AssertError(DomainErrors.Library.CannotScanLockedLibrary, ErrorType.Forbidden);
        AssertError(DomainErrors.Library.CannotScanDisabledLibrary, ErrorType.Forbidden);
        AssertError(DomainErrors.Library.LibraryIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Library.FilterMustIncludeLibraryId, ErrorType.Validation);
        AssertError(DomainErrors.Library.InvalidFilterAlphaKey, ErrorType.Validation);
        AssertError(DomainErrors.Library.LibraryAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.Library.LibraryNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Library.LibraryTypeCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Library.CoverFileMustBeAnImage, ErrorType.Validation);
        AssertError(DomainErrors.Library.UnknownLibraryType, ErrorType.Unexpected);
        AssertError(DomainErrors.Library.PathsListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Library.PathsListCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Library.TitleCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Library.TitleMustBeMaximum255CharactersLong, ErrorType.Validation);
    }

    [Fact]
    public void Users_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(3, typeof(DomainErrors.Users).GetProperties().Length);
        AssertError(DomainErrors.Users.UserAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.Users.UserDoesNotExist, ErrorType.NotFound);
        AssertError(DomainErrors.Users.UserIdCannotBeEmpty, ErrorType.Validation);
    }

    [Fact]
    public void UserSettings_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(3, typeof(DomainErrors.UserSettings).GetProperties().Length);
        AssertError(DomainErrors.UserSettings.UserSettingsAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.UserSettings.UserSettingsNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.UserSettings.ItemsPerPageMustBeGreaterThanZero, ErrorType.Validation);
    }

    [Fact]
    public void Metadata_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(31, typeof(DomainErrors.Metadata).GetProperties().Length);
        AssertError(DomainErrors.Metadata.MetadataCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.TitleCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.TitleMustBeMaximum255CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.OriginalTitleMustBeMaximum255CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.DescriptionMustBeMaximum2000CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.GenresListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.GenreNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.GenreNameMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.TagsListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.TagNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.TagNameMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.LanguageCodeCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.LanguageNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.LanguageCodeMustBe2CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.LanguageNameMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.OriginalReleaseYearMustBeBetween1And9999, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.ReReleaseYearMustBeBetween1And9999, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.CountryCodeMustBe2CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.OriginalReleaseDateAndYearMustMatch, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.ReReleaseDateAndYearMustMatch, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.ReleaseInfoCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingValueMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingMaxValueMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingValueCannotBeGreaterThanMaxValue, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingVoteCountMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingsListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.InvalidIsoCode, ErrorType.Validation);
    }

    [Fact]
    public void WrittenContent_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(27, typeof(DomainErrors.WrittenContent).GetProperties().Length);
        AssertError(DomainErrors.WrittenContent.BookAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.WrittenContent.IsbnValueCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.IsbnListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidIsbn10Format, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidIsbn13Format, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.UnknownIsbnFormat, ErrorType.Unexpected);
        AssertError(DomainErrors.WrittenContent.TheBookIsAlreadyInTheSeries, ErrorType.Forbidden);
        AssertError(DomainErrors.WrittenContent.TheBookIsNotInTheSeries, ErrorType.Forbidden);
        AssertError(DomainErrors.WrittenContent.AsinMustBe10CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.GoodreadsIdMustBeNumeric, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidLccnFormat, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidOclcFormat, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidOpenLibraryId, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.GoogleBooksIdMustBe12CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidGoogleBooksIdFormat, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidBarnesAndNoblesIdFormat, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidAppleBooksIdFormat, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.PublisherMustBeMaximum100CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.PageCountMustBeGreaterThanZero, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.UnknownBookFormat, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.EditionMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.VolumeNumberMustBeGreaterThanZero, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.BookLibraryCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.BookPathCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.BookNotFound, ErrorType.NotFound);
    }

    [Fact]
    public void MediaContributor_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(9, typeof(DomainErrors.MediaContributor).GetProperties().Length);
        AssertError(DomainErrors.MediaContributor.ContributorsListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorDisplayNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.RoleNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.RoleNameMustBeMaximum50CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.RoleCategoryCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorRoleCannotBeNull, ErrorType.Validation);
    }

    [Fact]
    public void MediaContributors_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(2, typeof(DomainErrors.MediaContributors).GetProperties().Length);
        AssertError(DomainErrors.MediaContributors.MediaContributorNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributors.MediaContributorRoleNameCannotBeEmpty, ErrorType.Validation);
    }

    [Fact]
    public void FileSystemManagement_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(19, typeof(DomainErrors.FileSystemManagement).GetProperties().Length);
        AssertError(DomainErrors.FileSystemManagement.ParentNodeCannotBeNull, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.PathMustBeMaximum260CharactersLong, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.FileCopyError, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.FileMoveError, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.DirectoryCopyError, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.DirectoryMoveError, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.FileNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.FileSystemManagement.InvalidPath, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.PathCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.ExtensionCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.StreamIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.CodecCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.BitrateMustBeAPositiveNumber, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.CannotNavigateUp, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.NameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.FileAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.FileSystemManagement.DirectoryNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.FileSystemManagement.DirectoryAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.FileSystemManagement.FileTooLarge, ErrorType.Validation);
    }

    [Fact]
    public void Reading_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(9, typeof(DomainErrors.Reading).GetProperties().Length);
        AssertError(DomainErrors.Reading.BookIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Reading.LocationRefCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Reading.ResourceKeyCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Reading.BookNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Reading.NoReaderAvailable, ErrorType.NotFound);
        AssertError(DomainErrors.Reading.ReaderDisabled, ErrorType.NotFound);
        AssertError(DomainErrors.Reading.BookFileNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Reading.SectionNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Reading.ResourceNotFound, ErrorType.NotFound);
    }

    [Fact]
    public void Themes_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(12, typeof(DomainErrors.Themes).GetProperties().Length);
        AssertError(DomainErrors.Themes.ThemeNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Themes.ThemeIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Themes.PageKeyCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Themes.ThemeAssetPathCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Themes.ThemeArchiveCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Themes.LastBundledThemeCannotBeDeleted, ErrorType.Forbidden);
        AssertError(DomainErrors.Themes.ThemeCannotBeDeleted, ErrorType.Forbidden);
        AssertError(DomainErrors.Themes.ThemeCannotBeRestored, ErrorType.Forbidden);
        AssertError(DomainErrors.Themes.ThemeArchiveNotReadable, ErrorType.Failure);
        AssertError(DomainErrors.Themes.ThemeFilesUnreadable, ErrorType.Failure);
        AssertError(DomainErrors.Themes.ThemeTemplateNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Themes.ThemeNotAvailable, ErrorType.Failure);
    }

    [Fact]
    public void Thumbnails_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(2, typeof(DomainErrors.Thumbnails).GetProperties().Length);
        AssertError(DomainErrors.Thumbnails.NoThumbnail, ErrorType.Failure);
        AssertError(DomainErrors.Thumbnails.ImageQualityMustBeBetweenZeroAndOneHundred, ErrorType.Validation);
    }

    [Fact]
    public void Permission_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Single(typeof(DomainErrors.Permission).GetProperties());
        AssertError(DomainErrors.Permission.UnauthorizedAccess, ErrorType.Failure);
    }

    [Fact]
    public void Plugins_WhenAccessed_ShouldExposeErrorsWithExpectedTypes()
    {
        // Assert
        // The count assertion fails when an error is added or removed, reminding to update the assertions below.
        Assert.Equal(12, typeof(DomainErrors.Plugins).GetProperties().Length);
        AssertError(DomainErrors.Plugins.PluginNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Plugins.PluginIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.PluginSettingsCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.LibraryIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.PluginIdsListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.PluginIdsListCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.LibraryMetadataProviderConfigurationNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Plugins.PluginArchiveCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.PluginFileNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.UnsupportedPluginFileType, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.PluginArchiveNotReadable, ErrorType.Failure);
        AssertError(DomainErrors.Plugins.PluginArchiveContainsNoAssemblies, ErrorType.Failure);
    }

    private static void AssertError(Error error, ErrorType expectedType)
    {
        Assert.Equal(expectedType, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Code));
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
    }
}
