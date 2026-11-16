#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ClearResumePoints;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.ClearResumePoints;

/// <summary>
/// Contains unit tests for the <see cref="ClearResumePointsCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsCommandValidatorTests
{
    private readonly ClearResumePointsCommandValidator _validator = new();
    private readonly ClearResumePointsCommandFixture _clearResumePointsCommandFixture = new();

    [Fact]
    public void Validate_WhenTargetUserIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(includeTargetUserId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

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
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(targetUserId: targetUserId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

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
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(libraryId: libraryId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNeitherTheTargetUserIdNorTheLibraryIdIsSupplied_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(includeTargetUserId: false, includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenOnlyTheTargetUserIdIsInvalid_ShouldReportOnlyTheTargetUserIdError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(targetUserId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOnlyTheLibraryIdIsInvalid_ShouldReportOnlyTheLibraryIdError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(libraryId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
