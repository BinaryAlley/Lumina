#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeAsset;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemeAsset;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeAssetQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetQueryValidatorTests
{
    private readonly GetThemeAssetQueryValidator _validator = new();
    private readonly GetThemeAssetQueryFixture _getThemeAssetQueryFixture = new();

    [Fact]
    public void Validate_WhenThemeIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        query = query with { ThemeId = null };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create(themeId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create(themeId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAssetPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();
        query = query with { AssetPath = null };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeAssetPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAssetPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create(assetPath: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeAssetPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAssetPathIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create(assetPath: "   ");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeAssetPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        GetThemeAssetQuery query = _getThemeAssetQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
