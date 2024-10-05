#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Files.Queries.GetFiles;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetFiles.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetFiles;

/// <summary>
/// Contains unit tests for the <see cref="GetFilesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesQueryValidatorTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly GetFilesQueryValidator _validator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesQueryValidatorTests"/> class.
    /// </summary>
    public GetFilesQueryValidatorTests()
    {
        _validator = new GetFilesQueryValidator();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetFilesQuery query = GetFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = null! };

        // Act
        TestValidationResult<GetFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetFilesQuery query = GetFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<GetFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetFilesQuery query = GetFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<GetFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetFilesQuery query = GetFilesQueryFixture.CreateGetFilesQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<GetFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetFilesQuery query = GetFilesQueryFixture.CreateGetFilesQuery();
        query = query with { IncludeHiddenElements = true };

        // Act
        TestValidationResult<GetFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetFilesQuery query = GetFilesQueryFixture.CreateGetFilesQuery();
        query = query with { IncludeHiddenElements = false };

        // Act
        TestValidationResult<GetFilesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeHiddenElements);
    }
    #endregion
}
