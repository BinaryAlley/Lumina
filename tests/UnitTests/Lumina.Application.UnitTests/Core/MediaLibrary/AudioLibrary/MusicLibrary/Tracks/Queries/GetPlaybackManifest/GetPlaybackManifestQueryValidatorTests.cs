#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaybackManifestQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestQueryValidatorTests
{
    private readonly GetPlaybackManifestQueryFixture _getPlaybackManifestQueryFixture = new();
    private readonly GetPlaybackManifestQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create();

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
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

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
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

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
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create(albumId: albumId, includeAlbumId: albumId is not null);

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
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create(trackId: trackId, includeTrackId: trackId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenSeveralFieldsAreInvalid_ShouldReportEachSpecificError()
    {
        // Arrange
        GetPlaybackManifestQuery query = _getPlaybackManifestQueryFixture.Create(
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
