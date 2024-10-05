#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectoryTree;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectoryTree.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectoryTree;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoryTreeQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoryTreeQueryValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly GetDirectoryTreeQueryValidator _validator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeQueryValidatorTests"/> class.
    /// </summary>
    public GetDirectoryTreeQueryValidatorTests()
    {
        _validator = new GetDirectoryTreeQueryValidator();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { Path = null! };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { IncludeHiddenElements = true };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { IncludeHiddenElements = false };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }

    [Fact]
    public void Validate_WhenIncludeFilesIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { IncludeFiles = true };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeFiles);
    }

    [Fact]
    public void Validate_WhenIncludeFilesIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetDirectoryTreeQuery query = GetDirectoryTreeQueryFixture.CreateGetDirectoryTreeQuery();
        query = query with { IncludeFiles = false };

        // Act
        TestValidationResult<GetDirectoryTreeQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeFiles);
    }
    #endregion
}
