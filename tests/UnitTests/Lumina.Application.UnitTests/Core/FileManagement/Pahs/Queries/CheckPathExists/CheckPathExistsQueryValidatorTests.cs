#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Paths.Queries.CheckPathExists;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.CheckPathExists.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.CheckPathExists;

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
        TestValidationResult<CheckPathExistsQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<CheckPathExistsQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<CheckPathExistsQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<CheckPathExistsQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }
}
