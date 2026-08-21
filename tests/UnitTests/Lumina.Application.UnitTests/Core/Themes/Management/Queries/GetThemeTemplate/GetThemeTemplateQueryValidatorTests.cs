#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetThemeTemplate;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeTemplate;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemeTemplate;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeTemplateQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateQueryValidatorTests
{
    private readonly GetThemeTemplateQueryValidator _validator = new();
    private readonly GetThemeTemplateQueryFixture _getThemeTemplateQueryFixture = new();

    [Fact]
    public void Validate_WhenThemeIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
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
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create(themeId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create(themeId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPageKeyIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();
        query = query with { PageKey = null };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.PageKeyCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPageKeyIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create(pageKey: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.PageKeyCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPageKeyIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create(pageKey: "   ");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.PageKeyCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        GetThemeTemplateQuery query = _getThemeTemplateQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
