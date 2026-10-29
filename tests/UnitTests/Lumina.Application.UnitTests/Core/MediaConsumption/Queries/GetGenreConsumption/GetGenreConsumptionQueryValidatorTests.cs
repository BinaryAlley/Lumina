#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Queries.GetGenreConsumption;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetGenreConsumption;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetGenreConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetGenreConsumptionQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionQueryValidatorTests
{
    private readonly GetGenreConsumptionQueryValidator _validator = new();
    private readonly GetGenreConsumptionQueryFixture _getGenreConsumptionQueryFixture = new();

    [Fact]
    public void Validate_WhenTargetUserIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(includeTargetUserId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Theory]
    [InlineData("")] // the target user Id is empty
    [InlineData("   ")] // the target user Id is whitespace only
    [InlineData("not-a-guid")] // the target user Id is not a Guid
    [InlineData("00000000-0000-0000-0000-000000000000")] // the target user Id is the empty Guid
    public void Validate_WhenTargetUserIdIsEmptyOrInvalid_ShouldHaveValidationError(string targetUserId)
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(targetUserId: targetUserId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(targetUserId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // the library Id is empty
    [InlineData("   ")] // the library Id is whitespace only
    [InlineData("not-a-guid")] // the library Id is not a Guid
    [InlineData("00000000-0000-0000-0000-000000000000")] // the library Id is the empty Guid
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string libraryId)
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(libraryId: libraryId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("")] // the media kind is empty
    [InlineData("   ")] // the media kind is whitespace only
    [InlineData("not-a-media-kind")] // the media kind is not a member name
    [InlineData("999")] // the media kind is numeric, which must not be accepted as a member name
    [InlineData("None")] // None is not a valid media kind for a consumption record
    public void Validate_WhenMediaKindIsInvalid_ShouldHaveValidationError(string mediaKind)
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData(nameof(MediaKind.MusicTrack))] // a kind supported by the current catalogues
    [InlineData(nameof(MediaKind.Book))] // another kind supported by the current catalogues
    [InlineData(nameof(MediaKind.Movie))] // a kind without a catalogue, still a defined kind
    [InlineData(nameof(MediaKind.Photo))] // a kind that every session counts for, still a defined kind
    [InlineData("musicTrack")] // member names are matched case-insensitively
    public void Validate_WhenMediaKindIsDefined_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("not-a-date")] // the start is not a date
    [InlineData("2026-13-01T00:00:00Z")] // the start is a date with an out of range month
    public void Validate_WhenFromUtcIsMalformed_ShouldHaveValidationError(string fromUtc)
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(
            fromUtc: fromUtc,
            includeToUtc: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Theory]
    [InlineData("not-a-date")] // the end is not a date
    [InlineData("2026-13-01T00:00:00Z")] // the end is a date with an out of range month
    public void Validate_WhenToUtcIsMalformed_ShouldHaveValidationError(string toUtc)
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(
            toUtc: toUtc,
            includeFromUtc: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenOnlyTheStartOfTheDateRangeIsSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(
            fromUtc: new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture),
            includeToUtc: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenOnlyTheEndOfTheDateRangeIsSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(
            toUtc: new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture),
            includeFromUtc: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenBothDateRangeBoundsAreEqual_ShouldNotHaveValidationError()
    {
        // Arrange
        string bound = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(fromUtc: bound, toUtc: bound);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenTheDateRangeIsInverted_ShouldHaveValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(
            fromUtc: new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture),
            toUtc: new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture));

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
