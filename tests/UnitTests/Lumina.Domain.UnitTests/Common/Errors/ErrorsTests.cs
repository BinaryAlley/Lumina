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
    public void Metadata_WhenAccessed_ShouldExposeRepresentativeErrorsWithExpectedTypes()
    {
        // Assert
        AssertError(DomainErrors.Metadata.MetadataCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.TitleCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.OriginalReleaseYearMustBeBetween1And9999, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.OriginalReleaseDateAndYearMustMatch, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingValueMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingValueCannotBeGreaterThanMaxValue, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.RatingVoteCountMustBePositive, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.InvalidIsoCode, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.GenreNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.TagNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Metadata.LanguageCodeCannotBeEmpty, ErrorType.Validation);
    }

    [Fact]
    public void WrittenContent_WhenAccessed_ShouldExposeRepresentativeErrorsWithExpectedTypes()
    {
        // Assert
        AssertError(DomainErrors.WrittenContent.BookAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.WrittenContent.IsbnValueCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidIsbn10Format, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.InvalidIsbn13Format, ErrorType.Validation);
        AssertError(DomainErrors.WrittenContent.UnknownIsbnFormat, ErrorType.Unexpected);
        AssertError(DomainErrors.WrittenContent.TheBookIsAlreadyInTheSeries, ErrorType.Forbidden);
        AssertError(DomainErrors.WrittenContent.TheBookIsNotInTheSeries, ErrorType.Forbidden);
        AssertError(DomainErrors.WrittenContent.BookNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.WrittenContent.PageCountMustBeGreaterThanZero, ErrorType.Validation);
    }

    [Fact]
    public void MediaContributor_WhenAccessed_ShouldExposeRepresentativeErrorsWithExpectedTypes()
    {
        // Assert
        AssertError(DomainErrors.MediaContributor.ContributorsListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorDisplayNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.RoleNameCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.RoleCategoryCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.MediaContributor.ContributorRoleCannotBeNull, ErrorType.Validation);
    }

    [Fact]
    public void FileSystemManagement_WhenAccessed_ShouldExposeRepresentativeErrorsWithExpectedTypes()
    {
        // Assert
        AssertError(DomainErrors.FileSystemManagement.ParentNodeCannotBeNull, ErrorType.Failure);
        AssertError(DomainErrors.FileSystemManagement.InvalidPath, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.PathCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.FileSystemManagement.FileNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.FileSystemManagement.FileAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.FileSystemManagement.DirectoryAlreadyExists, ErrorType.Conflict);
        AssertError(DomainErrors.FileSystemManagement.NameCannotBeEmpty, ErrorType.Validation);
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
    public void Plugins_WhenAccessed_ShouldExposeRepresentativeErrorsWithExpectedTypes()
    {
        // Assert
        AssertError(DomainErrors.Plugins.PluginNotFound, ErrorType.NotFound);
        AssertError(DomainErrors.Plugins.PluginIdCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.PluginIdsListCannotBeNull, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.PluginIdsListCannotBeEmpty, ErrorType.Validation);
        AssertError(DomainErrors.Plugins.LibraryMetadataProviderConfigurationNotFound, ErrorType.NotFound);
    }

    private static void AssertError(Error error, ErrorType expectedType)
    {
        Assert.Equal(expectedType, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Code));
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
    }
}
