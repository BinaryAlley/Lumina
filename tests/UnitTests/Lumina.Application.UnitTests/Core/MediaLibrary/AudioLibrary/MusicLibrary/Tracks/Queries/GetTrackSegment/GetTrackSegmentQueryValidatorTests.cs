#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackSegmentQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackSegmentQueryValidatorTests
{
    private const string VALID_DELIVERY_ID = "0123456789abcdef0123456789abcdef";

    private readonly GetTrackSegmentQueryFixture _getTrackSegmentQueryFixture = new();
    private readonly GetTrackSegmentQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(deliveryId: VALID_DELIVERY_ID, segmentIndex: "0");

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
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

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
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

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
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(albumId: albumId, includeAlbumId: albumId is not null);

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
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(trackId: trackId, includeTrackId: trackId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value
    public void Validate_WhenDeliveryIdIsEmpty_ShouldHaveDeliveryIdCannotBeEmptyError(string? deliveryId)
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(deliveryId: deliveryId, includeDeliveryId: deliveryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Playback.DeliveryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("0123456789ABCDEF0123456789ABCDEF")] // uppercase hexadecimal is not the canonical lowercase handle
    [InlineData("01234567-89ab-cdef-0123-456789abcdef")] // the D format of a Guid carries dashes
    [InlineData("0123456789abcdef0123456789abcde")] // 31 characters, one short of a handle
    [InlineData("0123456789abcdef0123456789abcdef0")] // 33 characters, one over a handle
    [InlineData("0123456789abcdef0123456789abcdeg")] // the last character is not a hexadecimal digit
    public void Validate_WhenDeliveryIdShapeIsInvalid_ShouldHaveDeliveryIdIsInvalidError(string deliveryId)
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(deliveryId: deliveryId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Playback.DeliveryIdIsInvalid);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value
    public void Validate_WhenSegmentIndexIsEmpty_ShouldHaveSegmentIndexCannotBeEmptyError(string? segmentIndex)
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(deliveryId: VALID_DELIVERY_ID, segmentIndex: segmentIndex, includeSegmentIndex: segmentIndex is not null);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Playback.SegmentIndexCannotBeEmpty);
    }

    [Theory]
    [InlineData("-1")] // a negative index does not address a segment
    [InlineData("abc")] // a non-numeric index does not address a segment
    [InlineData("1.5")] // a fractional index does not address a segment
    [InlineData("2147483648")] // a value above the range of an integer
    public void Validate_WhenSegmentIndexIsNotANonNegativeInteger_ShouldHaveSegmentIndexMustBeValidError(string segmentIndex)
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(deliveryId: VALID_DELIVERY_ID, segmentIndex: segmentIndex);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Playback.SegmentIndexMustBeValid);
    }

    [Theory]
    [InlineData("0")] // the first segment of the audio
    [InlineData("2147483647")] // the largest index an integer can address
    public void Validate_WhenSegmentIndexIsANonNegativeInteger_ShouldNotHaveSegmentIndexMustBeValidError(string segmentIndex)
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(deliveryId: VALID_DELIVERY_ID, segmentIndex: segmentIndex);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Playback.SegmentIndexMustBeValid);
        result.ShouldNotHaveValidationError(Errors.Playback.SegmentIndexCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenSeveralFieldsAreInvalid_ShouldReportEachSpecificError()
    {
        // Arrange
        GetTrackSegmentQuery query = _getTrackSegmentQueryFixture.Create(
            libraryId: "not-a-library-guid",
            artistId: null,
            includeArtistId: false,
            albumId: Guid.Empty.ToString(),
            trackId: "not-a-track-guid",
            deliveryId: "0123456789ABCDEF0123456789ABCDEF",
            segmentIndex: "-1");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
        result.ShouldHaveValidationError(Errors.Playback.DeliveryIdIsInvalid);
        result.ShouldHaveValidationError(Errors.Playback.SegmentIndexMustBeValid);
        Assert.Equal(6, result.Count);
    }
}
