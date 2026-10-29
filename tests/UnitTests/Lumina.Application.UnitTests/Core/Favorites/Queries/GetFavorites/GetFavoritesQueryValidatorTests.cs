#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Favorites.Queries.GetFavorites;
using Lumina.Application.Fixtures.Core.Favorites.Queries.GetFavorites;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Favorites.Queries.GetFavorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoritesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesQueryValidatorTests
{
    private readonly GetFavoritesQueryValidator _validator = new();
    private readonly GetFavoritesQueryFixture _getFavoritesQueryFixture = new();

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldNotHaveValidationError()
    {
        // Arrange
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaKindIsUndefined_ShouldHaveValidationError()
    {
        // Arrange
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create(mediaKind: (FavoriteMediaKind)int.MaxValue);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNone_ShouldHaveValidationError()
    {
        // Arrange
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create(mediaKind: FavoriteMediaKind.None);

        // Act
        List<Error> result = _validator.TestValidate(query);

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
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create(mediaKind: mediaKind);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }
}
