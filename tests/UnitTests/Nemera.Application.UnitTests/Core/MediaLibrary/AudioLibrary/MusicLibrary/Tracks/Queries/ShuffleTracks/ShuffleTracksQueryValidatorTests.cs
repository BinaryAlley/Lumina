#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;

/// <summary>
/// Contains unit tests for the <see cref="ShuffleTracksQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksQueryValidatorTests
{
    private readonly ShuffleTracksQueryFixture _shuffleTracksQueryFixture = new();
    private readonly ShuffleTracksQueryValidator _validator = new();

    [Theory]
    [InlineData(1)] // The smallest allowed selection.
    [InlineData(500)] // The largest allowed selection.
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError(int count)
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(
            count: count,
            includeArtistId: false,
            includeAlbumId: false,
            includeExcludedTrackIds: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.ShuffleTrackCountMustBeGreaterThanZero);
        result.ShouldNotHaveValidationError(Errors.Music.ShuffleTrackCountMustBeAtMost500);
    }

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value.
    [InlineData("not-a-guid")] // Non-Guid route value.
    [InlineData("' OR '1'='1")] // Injection payload that is not a Guid.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(0)] // No track is requested.
    [InlineData(-1)] // A negative selection size.
    [InlineData(-500)] // A large negative selection size.
    public void Validate_WhenCountIsNotPositive_ShouldHaveValidationError(int count)
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(count: count);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ShuffleTrackCountMustBeGreaterThanZero);
        result.ShouldNotHaveValidationError(Errors.Music.ShuffleTrackCountMustBeAtMost500);
    }

    [Theory]
    [InlineData(501)] // One more than the documented maximum.
    [InlineData(1000)] // Well above the documented maximum.
    [InlineData(int.MaxValue)] // The largest possible value.
    public void Validate_WhenCountExceedsMaximum_ShouldHaveValidationError(int count)
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(count: count);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ShuffleTrackCountMustBeAtMost500);
        result.ShouldNotHaveValidationError(Errors.Music.ShuffleTrackCountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenArtistIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(artistId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(includeArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenAlbumIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(albumId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(includeAlbumId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenExcludedTrackIdsAreNull_ShouldNotHaveValidationError()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(includeExcludedTrackIds: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        // The validator has no rule for the exclusion list, because excluding tracks can never make a valid selection invalid.
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenExcludedTrackIdsAreEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(excludedTrackIds: []);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenExcludedTrackIdsContainEmptyOrDuplicatedIds_ShouldNotHaveValidationError()
    {
        // Arrange
        Guid trackId = Guid.NewGuid();
        ShuffleTracksQuery query = _shuffleTracksQueryFixture.Create(excludedTrackIds: [trackId, Guid.Empty, trackId]);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        // The exclusion list is normalized by the handler instead, so the validator reports no error for empty or repeated entries.
        result.ShouldNotHaveAnyValidationErrors();
    }
}
