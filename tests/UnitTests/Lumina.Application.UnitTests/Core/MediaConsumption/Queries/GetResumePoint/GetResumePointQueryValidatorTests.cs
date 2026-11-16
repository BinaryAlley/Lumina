#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Queries.GetResumePoint;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetResumePoint;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetResumePoint;

/// <summary>
/// Contains unit tests for the <see cref="GetResumePointQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointQueryValidatorTests
{
    private readonly GetResumePointQueryValidator _validator = new();
    private readonly GetResumePointQueryFixture _getResumePointQueryFixture = new();

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("")] // The media kind is empty.
    [InlineData("   ")] // The media kind is whitespace only.
    [InlineData("not-a-media-kind")] // The media kind is not the name of a defined kind.
    [InlineData("99999")] // A numeric value that is not the name of a defined kind.
    [InlineData("1")] // A numeric value that parses to the defined MusicTrack member, which the old Enum.TryParse and Enum.IsDefined logic accepted.
    public void Validate_WhenMediaKindIsNotTheNameOfADefinedKind_ShouldHaveValidationError(string mediaKind)
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData(nameof(MediaKind.MusicTrack))] // A kind supported by the current catalogues.
    [InlineData(nameof(MediaKind.Book))] // Another kind supported by the current catalogues.
    [InlineData(nameof(MediaKind.Movie))] // A kind without a catalogue, still a defined kind.
    [InlineData(nameof(MediaKind.Photo))] // A kind that cannot be resumed, still a defined kind.
    public void Validate_WhenMediaKindIsTheNameOfADefinedKind_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Theory]
    [InlineData("musictrack")] // The name of a defined kind, in lower case.
    [InlineData("MUSICTRACK")] // The name of a defined kind, in upper case.
    [InlineData("musicTrack")] // The name of a defined kind, in mixed case.
    public void Validate_WhenMediaKindIsTheNameOfADefinedKindInAnotherCase_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.UnsupportedMediaKind);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create(includeMediaItemId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

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
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaConsumption.MediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
