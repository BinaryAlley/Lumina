#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;

/// <summary>
/// Contains unit tests for the <see cref="GetAllUsersConsumptionStatisticsQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsQueryValidatorTests
{
    private readonly GetAllUsersConsumptionStatisticsQueryValidator _validator = new();
    private readonly GetAllUsersConsumptionStatisticsQueryFixture _getAllUsersConsumptionStatisticsQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(includeLibraryId: false);

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
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(libraryId: libraryId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNone_ShouldHaveValidationError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(mediaKind: MediaKind.None);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaKindIsUndefined_ShouldHaveValidationError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(mediaKind: (MediaKind)99);

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
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenOnlyTheStartOfTheDateRangeIsSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(
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
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(
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
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(fromUtc: bound, toUtc: bound);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.ConsumptionDateRangeIsInvalid);
    }

    [Fact]
    public void Validate_WhenTheDateRangeIsInverted_ShouldHaveValidationError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(
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
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
