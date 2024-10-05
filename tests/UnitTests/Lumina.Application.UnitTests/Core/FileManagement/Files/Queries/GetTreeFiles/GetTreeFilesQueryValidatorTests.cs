#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Files.Queries.GetTreeFiles;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetTreeFiles.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesQueryValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly GetTreeFilesQueryValidator _validator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryValidatorTests"/> class.
    /// </summary>
    public GetTreeFilesQueryValidatorTests()
    {
        _validator = new GetTreeFilesQueryValidator();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = GetTreeFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = null! };

        // Act
        TestValidationResult<GetTreeFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = GetTreeFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<GetTreeFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = GetTreeFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<GetTreeFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = GetTreeFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<GetTreeFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = GetTreeFilesQueryFixture.CreateGetFilesQuery();
        query = query with { IncludeHiddenElements = true };

        // Act
        TestValidationResult<GetTreeFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = GetTreeFilesQueryFixture.CreateGetFilesQuery();
        query = query with { IncludeHiddenElements = false };

        // Act
        TestValidationResult<GetTreeFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }
    #endregion
}
