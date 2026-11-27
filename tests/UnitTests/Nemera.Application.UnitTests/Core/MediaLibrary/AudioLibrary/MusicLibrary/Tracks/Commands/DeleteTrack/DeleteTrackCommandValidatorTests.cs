#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

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
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value, which both the not empty rule and the format rule reject.
    [InlineData("not-a-library-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
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
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value, which both the not empty rule and the format rule reject.
    [InlineData("not-an-artist-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
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
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value, which both the not empty rule and the format rule reject.
    [InlineData("not-an-album-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
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
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value, which both the not empty rule and the format rule reject.
    [InlineData("not-a-track-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
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
