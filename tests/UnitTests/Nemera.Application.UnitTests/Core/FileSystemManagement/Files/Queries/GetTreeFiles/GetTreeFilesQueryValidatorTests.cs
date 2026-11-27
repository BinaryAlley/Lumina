#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Nemera.Application.Fixtures.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesQueryValidatorTests
{
    private readonly GetTreeFilesQueryValidator _validator = new();
    private readonly GetTreeFilesQueryFixture _getTreeFilesQueryFixture = new();

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
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
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
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
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
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
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenShouldIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        query = query with { ShouldIncludeHiddenElements = true };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenShouldIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        query = query with { ShouldIncludeHiddenElements = false };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
