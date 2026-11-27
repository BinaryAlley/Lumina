#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaConsumption.Queries.GetTagConsumption;
using Nemera.Application.Fixtures.Core.MediaConsumption.Queries.GetTagConsumption;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaConsumption.Queries.GetTagConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetTagConsumptionQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTagConsumptionQueryValidatorTests
{
    private readonly GetTagConsumptionQueryValidator _validator = new();
    private readonly GetTagConsumptionQueryFixture _getTagConsumptionQueryFixture = new();

    [Fact]
    public void Validate_WhenTargetUserIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(includeTargetUserId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Theory]
    [InlineData("")] // The target user Id is empty.
    [InlineData("   ")] // The target user Id is whitespace only.
    [InlineData("not-a-guid")] // The target user Id is not a Guid.
    [InlineData("00000000-0000-0000-0000-000000000000")] // The target user Id is the empty Guid.
    public void Validate_WhenTargetUserIdIsEmptyOrInvalid_ShouldHaveValidationError(string targetUserId)
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(targetUserId: targetUserId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(targetUserId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // The library Id is empty.
    [InlineData("   ")] // The library Id is whitespace only.
    [InlineData("not-a-guid")] // The library Id is not a Guid.
    [InlineData("00000000-0000-0000-0000-000000000000")] // The library Id is the empty Guid.
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string libraryId)
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(libraryId: libraryId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNone_ShouldHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(mediaKind: nameof(MediaKind.None));

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaKindIsUndefined_ShouldHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(mediaKind: "999");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData(nameof(MediaKind.MusicTrack))] // A kind supported by the current catalogues.
    [InlineData(nameof(MediaKind.Book))] // Another kind supported by the current catalogues.
    [InlineData(nameof(MediaKind.Movie))] // A kind without a catalogue, still a defined kind.
    [InlineData(nameof(MediaKind.Photo))] // A kind that every session counts for, still a defined kind.
    public void Validate_WhenMediaKindIsDefined_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenOnlyTheStartOfTheDateRangeIsSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(
            fromUtc: "2026-03-01T00:00:00Z",
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
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(
            toUtc: "2026-03-02T00:00:00Z",
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
        string bound = "2026-03-02T00:00:00Z";
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(fromUtc: bound, toUtc: bound);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenTheDateRangeIsInverted_ShouldHaveValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(
            fromUtc: "2026-03-02T00:00:00Z",
            toUtc: "2026-03-01T00:00:00Z");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Theory]
    [InlineData("")] // An empty bound.
    [InlineData("not-a-date")] // A textual value that is not a date.
    [InlineData("2026-13-45T99:99:99")] // A value with out of range components.
    public void Validate_WhenADateRangeBoundIsNotAValidDate_ShouldHaveValidationError(string invalidBound)
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create(fromUtc: invalidBound);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
