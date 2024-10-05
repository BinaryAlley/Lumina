#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Paths.Queries.ValidatePath;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.ValidatePath.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.ValidatePath;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathQueryValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ValidatePathQueryValidator _validator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryValidatorTests"/> class.
    /// </summary>
    public ValidatePathQueryValidatorTests()
    {
        _validator = new ValidatePathQueryValidator();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        query = query with { Path = null! };

        // Act
        TestValidationResult<ValidatePathQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<ValidatePathQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<ValidatePathQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        ValidatePathQuery query = ValidatePathQueryFixure.CreateValidatePathQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<ValidatePathQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }
    #endregion
}
