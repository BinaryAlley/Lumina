#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackQueryValidatorTests
{
    private readonly GetTrackQueryFixture _getTrackQueryFixture = new();
    private readonly GetTrackQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTrackQuery query = _getTrackQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
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
        GetTrackQuery query = _getTrackQueryFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

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
        GetTrackQuery query = _getTrackQueryFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

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
        GetTrackQuery query = _getTrackQueryFixture.Create(albumId: albumId, includeAlbumId: albumId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

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
        GetTrackQuery query = _getTrackQueryFixture.Create(trackId: trackId, includeTrackId: trackId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }
}
