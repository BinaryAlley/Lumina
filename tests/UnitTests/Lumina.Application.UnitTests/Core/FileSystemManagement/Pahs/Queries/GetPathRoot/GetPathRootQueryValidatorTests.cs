#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathRoot.Fixtures;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathRoot;

/// <summary>
/// Contains unit tests for the <see cref="GetPathRootQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootQueryValidatorTests
{
    private readonly GetPathRootQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryValidatorTests"/> class.
    /// </summary>
    public GetPathRootQueryValidatorTests()
    {
        _validator = new GetPathRootQueryValidator();
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
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
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
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
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
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
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
