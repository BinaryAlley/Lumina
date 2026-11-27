#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Nemera.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Contains unit tests for the <see cref="GetPathRootQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootQueryValidatorTests
{
    private readonly GetPathRootQueryValidator _validator = new();
    private readonly GetPathRootQueryFixture _getPathRootQueryFixture = new();

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetPathRootQuery query = _getPathRootQueryFixture.Create();
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
        GetPathRootQuery query = _getPathRootQueryFixture.Create();
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
        GetPathRootQuery query = _getPathRootQueryFixture.Create();
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
        GetPathRootQuery query = _getPathRootQueryFixture.Create();
        query = query with { Path = "/valid/path" };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
