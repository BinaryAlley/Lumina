#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Queries.GetRecentlyConsumed;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetRecentlyConsumed;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetRecentlyConsumed;

/// <summary>
/// Contains unit tests for the <see cref="GetRecentlyConsumedQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRecentlyConsumedQueryValidatorTests
{
    private readonly GetRecentlyConsumedQueryValidator _validator = new();
    private readonly GetRecentlyConsumedQueryFixture _getRecentlyConsumedQueryFixture = new();

    [Fact]
    public void Validate_WhenTargetUserIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(includeTargetUserId: false);

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
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(targetUserId: targetUserId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(targetUserId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(includeLibraryId: false);

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
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(libraryId: libraryId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNone_ShouldHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(mediaKind: MediaKind.None);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaKindIsUndefined_ShouldHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(mediaKind: (MediaKind)99);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData(MediaKind.MusicTrack)] // a kind supported by the current catalogues
    [InlineData(MediaKind.Book)] // another kind supported by the current catalogues
    [InlineData(MediaKind.Movie)] // a kind without a catalogue, still a defined kind
    [InlineData(MediaKind.Photo)] // a kind that every session counts for, still a defined kind
    public void Validate_WhenMediaKindIsDefined_ShouldNotHaveValidationError(MediaKind mediaKind)
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenOnlyTheStartOfTheDateRangeIsSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(
            fromUtc: DateTime.UtcNow.AddDays(-7),
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
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(
            toUtc: DateTime.UtcNow,
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
        DateTime bound = DateTime.UtcNow;
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(fromUtc: bound, toUtc: bound);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenTheDateRangeIsInverted_ShouldHaveValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create(
            fromUtc: DateTime.UtcNow,
            toUtc: DateTime.UtcNow.AddDays(-1));

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetRecentlyConsumedQuery query = _getRecentlyConsumedQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
