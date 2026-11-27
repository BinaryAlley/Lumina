#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Nemera.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsQueryValidatorTests
{
    private readonly CheckPathExistsQueryValidator _validator = new();
    private readonly CheckPathExistsQueryFixture _checkPathExistsQueryFixture = new();

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        CheckPathExistsQuery query = _checkPathExistsQueryFixture.Create();
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
        CheckPathExistsQuery query = _checkPathExistsQueryFixture.Create();
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
        CheckPathExistsQuery query = _checkPathExistsQueryFixture.Create();
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
        CheckPathExistsQuery query = _checkPathExistsQueryFixture.Create();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
