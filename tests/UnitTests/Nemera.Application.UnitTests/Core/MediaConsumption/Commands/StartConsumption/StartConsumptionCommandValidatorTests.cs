#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaConsumption.Commands.StartConsumption;
using Nemera.Application.Fixtures.Core.MediaConsumption.Commands.StartConsumption;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaConsumption.Commands.StartConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StartConsumptionCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionCommandValidatorTests
{
    private readonly StartConsumptionCommandValidator _validator = new();
    private readonly StartConsumptionCommandFixture _startConsumptionCommandFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(includeLibraryId: false);

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
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(libraryId: libraryId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("")] // The media kind is empty.
    [InlineData("   ")] // The media kind is whitespace only.
    [InlineData("Hologram")] // The media kind is not a member of the enumeration.
    [InlineData("0")] // The media kind is a numeric route value, which the validator rejects by name.
    [InlineData("1")] // A numeric route value that parses to the defined MusicTrack member, which the old Enum.TryParse and Enum.IsDefined logic accepted.
    public void Validate_WhenMediaKindIsEmptyOrUnsupported_ShouldHaveValidationError(string mediaKind)
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("None")] // The default enumeration member is still a member name.
    [InlineData("MusicTrack")]
    [InlineData("PodcastEpisode")]
    [InlineData("Audiobook")]
    [InlineData("Movie")]
    [InlineData("TvEpisode")]
    [InlineData("Book")]
    [InlineData("Photo")]
    [InlineData("Video")]
    public void Validate_WhenMediaKindIsAMemberName_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("musictrack")] // The member name is lower case.
    [InlineData("BoOk")] // The member name is mixed case.
    public void Validate_WhenMediaKindUsesDifferentCasing_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(includeMediaItemId: false);

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
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaItemId: mediaItemId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
