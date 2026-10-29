#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Favorites.Commands.RemoveFavorite;
using Lumina.Application.Fixtures.Core.Favorites.Commands.RemoveFavorite;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Contains unit tests for the <see cref="RemoveFavoriteCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteCommandValidatorTests
{
    private readonly RemoveFavoriteCommandValidator _validator = new();
    private readonly RemoveFavoriteCommandFixture _removeFavoriteCommandFixture = new();

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Theory]
    [InlineData("")] // the media kind is empty
    [InlineData("   ")] // the media kind is whitespace only
    [InlineData("Hologram")] // the media kind is not a member of the enumeration
    [InlineData("None")] // the default value is not a kind that can be favorited
    [InlineData("none")] // the default value, matched case insensitively
    [InlineData("0")] // the media kind is a numeric route value, which the validator rejects by name
    [InlineData("1")] // a numeric route value that parses to a defined member, which the name check rejects
    [InlineData("MusicTrack,Book")] // a comma separated list is not a single member name
    [InlineData("../MusicTrack")] // a path-like value is not a member name
    public void Validate_WhenMediaKindIsEmptyOrNotAMemberName_ShouldHaveValidationError(string mediaKind)
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Theory]
    [InlineData("musictrack")] // the member name is lower case
    [InlineData("BoOk")] // the member name is mixed case
    public void Validate_WhenMediaKindUsesDifferentCasing_ShouldNotHaveValidationError(string mediaKind)
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create(includeMediaItemId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // the media item Id is empty
    [InlineData("   ")] // the media item Id is whitespace only
    [InlineData("not-a-guid")] // the media item Id is not a Guid
    [InlineData("00000000-0000-0000-0000-000000000000")] // the media item Id is the empty Guid
    public void Validate_WhenMediaItemIdIsEmptyOrInvalid_ShouldHaveValidationError(string mediaItemId)
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create(mediaItemId: mediaItemId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenBothValuesAreInvalid_ShouldHaveBothValidationErrors()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create(
            mediaKind: "Hologram",
            mediaItemId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }
}
