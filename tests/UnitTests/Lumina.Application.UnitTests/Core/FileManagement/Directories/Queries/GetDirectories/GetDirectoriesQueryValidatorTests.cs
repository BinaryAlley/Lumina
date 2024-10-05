#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectories;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectories.Fixtures;
using System.Diagnostics.CodeAnalysis;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesQueryValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly GetDirectoriesQueryValidator _validator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryValidatorTests"/> class.
    /// </summary>
    public GetDirectoriesQueryValidatorTests()
    {
        _validator = new GetDirectoriesQueryValidator();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery();
        query = query with { Path = null! };

        // Act
        TestValidationResult<GetDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<GetDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<GetDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<GetDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery();
        query = query with { IncludeHiddenElements = true };

        // Act
        TestValidationResult<GetDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoriesQuery query = GetDirectoriesQueryFixture.CreateGetDirectoriesQuery();
        query = query with { IncludeHiddenElements = false };

        // Act
        TestValidationResult<GetDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }
    #endregion
}
