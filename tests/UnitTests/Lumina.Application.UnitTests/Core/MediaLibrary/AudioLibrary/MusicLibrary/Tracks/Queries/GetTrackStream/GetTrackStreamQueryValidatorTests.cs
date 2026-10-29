#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackStreamQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackStreamQueryValidatorTests
{
    private readonly GetTrackStreamQueryFixture _getTrackStreamQueryFixture = new();
    private readonly GetTrackStreamQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value
    [InlineData("not-a-library-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveLibraryIdError(string? libraryId)
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value
    [InlineData("not-an-artist-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenArtistIdIsEmptyOrInvalid_ShouldHaveArtistIdError(string? artistId)
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value
    [InlineData("not-an-album-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenAlbumIdIsEmptyOrInvalid_ShouldHaveAlbumIdError(string? albumId)
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create(albumId: albumId, includeAlbumId: albumId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value
    [InlineData("not-a-track-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenTrackIdIsEmptyOrInvalid_ShouldHaveTrackIdError(string? trackId)
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create(trackId: trackId, includeTrackId: trackId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenSeveralFieldsAreInvalid_ShouldReportEachSpecificError()
    {
        // Arrange
        GetTrackStreamQuery query = _getTrackStreamQueryFixture.Create(
            libraryId: "not-a-library-guid",
            artistId: null,
            includeArtistId: false,
            albumId: Guid.Empty.ToString(),
            trackId: "not-a-track-guid");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
        Assert.Equal(4, result.Count);
    }
}
