#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Application.Fixtures.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.Favorites.Queries.GetFavoriteStatus;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoriteStatusQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusQueryValidatorTests
{
    private readonly GetFavoriteStatusQueryValidator _validator = new();
    private readonly GetFavoriteStatusQueryFixture _getFavoriteStatusQueryFixture = new();

    [Fact]
    public void Validate_WhenMediaKindIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(includeMediaKind: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaKindIsUndefined_ShouldHaveValidationError()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(mediaKind: (FavoriteMediaKind)int.MaxValue);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaKindIsNone_ShouldHaveValidationError()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(mediaKind: FavoriteMediaKind.None);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaKindIsInvalid);
    }

    [Fact]
    public void Validate_WhenMediaItemIdsAreNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(includeMediaItemIds: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdsCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMediaItemIdsAreEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(mediaItemIds: []);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdsCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThereAreMoreThanOneHundredMediaItemIds_ShouldHaveMaximumCountValidationError()
    {
        // Arrange
        List<Guid> mediaItemIds = [.. Enumerable.Range(0, 101).Select(_ => Guid.NewGuid())];
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(mediaItemIds: mediaItemIds);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdsExceedMaximumCount);
    }

    [Fact]
    public void Validate_WhenThereAreExactlyOneHundredMediaItemIds_ShouldNotHaveMaximumCountValidationError()
    {
        // Arrange
        List<Guid> mediaItemIds = [.. Enumerable.Range(0, 100).Select(_ => Guid.NewGuid())];
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(mediaItemIds: mediaItemIds);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Favorites.FavoriteMediaItemIdsExceedMaximumCount);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenOneMediaItemIdIsTheEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        List<Guid> mediaItemIds = [Guid.NewGuid(), Guid.Empty];
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(mediaItemIds: mediaItemIds);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
