#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.CheckPathExists.Fixtures;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.CheckPathExists;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsQueryValidatorTests
{
    private readonly CheckPathExistsQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsQueryValidatorTests"/> class.
    /// </summary>
    public CheckPathExistsQueryValidatorTests()
    {
        _validator = new CheckPathExistsQueryValidator();
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
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
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
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
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
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
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
