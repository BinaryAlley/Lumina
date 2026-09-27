#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Contains unit tests for the <see cref="DeleteTrackCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteTrackCommandValidatorTests
{
    private readonly DeleteTrackCommandFixture _deleteTrackCommandFixture = new();
    private readonly DeleteTrackCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenIdsAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-a-library-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-an-artist-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenArtistIdIsEmptyOrInvalid_ShouldHaveValidationError(string? artistId)
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-an-album-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenAlbumIdIsEmptyOrInvalid_ShouldHaveValidationError(string? albumId)
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create(albumId: albumId, includeAlbumId: albumId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-a-track-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenTrackIdIsEmptyOrInvalid_ShouldHaveValidationError(string? trackId)
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create(trackId: trackId, includeTrackId: trackId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOnlyTheArtistIdIsInvalid_ShouldNotReportUnrelatedValidationErrors()
    {
        // Arrange
        DeleteTrackCommand command = _deleteTrackCommandFixture.Create(artistId: "not-an-artist-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }
}
