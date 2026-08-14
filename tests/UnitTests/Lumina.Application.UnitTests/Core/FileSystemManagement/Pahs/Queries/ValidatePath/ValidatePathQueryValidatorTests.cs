#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.ValidatePath.Fixtures;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.ValidatePath;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathQueryValidatorTests
{
    private readonly ValidatePathQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryValidatorTests"/> class.
    /// </summary>
    public ValidatePathQueryValidatorTests()
    {
        _validator = new ValidatePathQueryValidator();
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
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
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
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
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
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
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
