#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetTreeDirectories;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetTreeDirectories.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetTreeDirectories;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeDirectoriesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeDirectoriesQueryValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly GetTreeDirectoriesQueryValidator _validator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesQueryValidatorTests"/> class.
    /// </summary>
    public GetTreeDirectoriesQueryValidatorTests()
    {
        _validator = new GetTreeDirectoriesQueryValidator();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery();
        query = query with { Path = null! };

        // Act
        TestValidationResult<GetTreeDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<GetTreeDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<GetTreeDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<GetTreeDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery();
        query = query with { IncludeHiddenElements = true };

        // Act
        TestValidationResult<GetTreeDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeDirectoriesQuery query = GetTreeDirectoriesQueryFixture.CreateGetTreeDirectoryQuery();
        query = query with { IncludeHiddenElements = false };

        // Act
        TestValidationResult<GetTreeDirectoriesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }
    #endregion
}
