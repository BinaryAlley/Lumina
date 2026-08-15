#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesQueryValidatorTests
{
    private readonly GetDirectoriesQueryValidator _validator = new();
    private readonly GetDirectoriesQueryFixture _getDirectoriesQueryFixture = new();

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        query = query with { Path = null! };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        query = query with { Path = string.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        query = query with { Path = "   " };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        query = query with { IncludeHiddenElements = true };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = _getDirectoriesQueryFixture.Create();
        query = query with { IncludeHiddenElements = false };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
