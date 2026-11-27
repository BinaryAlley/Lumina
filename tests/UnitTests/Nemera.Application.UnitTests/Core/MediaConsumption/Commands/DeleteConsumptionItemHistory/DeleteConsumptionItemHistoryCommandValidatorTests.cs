#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;
using Nemera.Application.Fixtures.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;

/// <summary>
/// Contains unit tests for the <see cref="DeleteConsumptionItemHistoryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryCommandValidatorTests
{
    private readonly DeleteConsumptionItemHistoryCommandValidator _validator = new();
    private readonly DeleteConsumptionItemHistoryCommandFixture _deleteConsumptionItemHistoryCommandFixture = new();

    [Fact]
    public void Validate_WhenTargetUserIdIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(includeTargetUserId: false);

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
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(targetUserId: targetUserId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetUserIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(targetUserId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("")] // The media kind is empty.
    [InlineData("   ")] // The media kind is whitespace only.
    [InlineData("not-a-media-kind")] // The media kind is not a member name.
    [InlineData("999")] // The media kind is numeric, which must not be accepted as a member name.
    [InlineData("None")] // None is not a valid media kind for a consumption record.
    public void Validate_WhenMediaKindIsInvalid_ShouldHaveValidationError(string mediaKind)
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData(nameof(MediaKind.MusicTrack))] // A kind supported by the current catalogues.
    [InlineData(nameof(MediaKind.Book))] // Another kind supported by the current catalogues.
    [InlineData(nameof(MediaKind.Movie))] // A kind without a catalogue, still a defined kind.
    [InlineData(nameof(MediaKind.Photo))] // A kind that every session counts for, still a defined kind.
    [InlineData("musicTrack")] // Member names are matched case-insensitively.
    public void Validate_WhenMediaKindIsDefined_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(includeMediaItemId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // The media item Id is empty.
    [InlineData("   ")] // The media item Id is whitespace only.
    [InlineData("not-a-guid")] // The media item Id is not a Guid.
    [InlineData("00000000-0000-0000-0000-000000000000")] // The media item Id is the empty Guid.
    public void Validate_WhenMediaItemIdIsEmptyOrInvalid_ShouldHaveValidationError(string mediaItemId)
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(mediaItemId: mediaItemId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(mediaItemId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // The library Id is empty.
    [InlineData("   ")] // The library Id is whitespace only.
    [InlineData("not-a-guid")] // The library Id is not a Guid.
    [InlineData("00000000-0000-0000-0000-000000000000")] // The library Id is the empty Guid.
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string libraryId)
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(libraryId: libraryId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsAValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
