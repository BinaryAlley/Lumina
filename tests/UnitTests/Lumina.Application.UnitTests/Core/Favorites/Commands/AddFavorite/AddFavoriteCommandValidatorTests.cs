#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Favorites.Commands.AddFavorite;
using Lumina.Application.Fixtures.Core.Favorites.Commands.AddFavorite;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Favorites.Commands.AddFavorite;

/// <summary>
/// Contains unit tests for the <see cref="AddFavoriteCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteCommandValidatorTests
{
    private readonly AddFavoriteCommandValidator _validator = new();
    private readonly AddFavoriteCommandFixture _addFavoriteCommandFixture = new();

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaKindIsUndefined_ShouldHaveValidationError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: (FavoriteMediaKind)int.MaxValue);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNone_ShouldHaveValidationError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: FavoriteMediaKind.None);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist)] // a catalogued music kind
    [InlineData(FavoriteMediaKind.MusicAlbum)] // a catalogued music kind
    [InlineData(FavoriteMediaKind.MusicTrack)] // a catalogued music kind
    [InlineData(FavoriteMediaKind.Book)] // a catalogued written content kind
    [InlineData(FavoriteMediaKind.Video)] // a planned kind is still a defined enumeration member
    [InlineData(FavoriteMediaKind.Photo)] // a planned kind is still a defined enumeration member
    public void Validate_WhenMediaKindIsDefined_ShouldNotHaveValidationError(FavoriteMediaKind mediaKind)
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(includeMediaItemId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaItemIdIsTheEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaItemId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
