#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesQueryValidatorTests
{
    private readonly GetTreeFilesQueryValidator _validator;
    private readonly GetTreeFilesQueryFixture _getTreeFilesQueryFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryValidatorTests"/> class.
    /// </summary>
    public GetTreeFilesQueryValidatorTests()
    {
        _validator = new GetTreeFilesQueryValidator();
        _getTreeFilesQueryFixture = new GetTreeFilesQueryFixture();
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
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
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
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
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
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
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsTrue_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        query = query with { IncludeHiddenElements = true };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenIncludeHiddenElementsIsFalse_ShouldNotHaveValidationError()
    {
        // Arrange
        GetTreeFilesQuery query = _getTreeFilesQueryFixture.Create();
        query = query with { IncludeHiddenElements = false };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
