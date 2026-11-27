#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeArchive;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemeArchive;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeArchiveQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveQueryValidatorTests
{
    private readonly GetThemeArchiveQueryValidator _validator = new();
    private readonly GetThemeArchiveQueryFixture _getThemeArchiveQueryFixture = new();

    [Fact]
    public void Validate_WhenThemeIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();
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
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create(themeId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsWhiteSpace_ShouldHaveValidationError()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create(themeId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Themes.ThemeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenThemeIdIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        GetThemeArchiveQuery query = _getThemeArchiveQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
