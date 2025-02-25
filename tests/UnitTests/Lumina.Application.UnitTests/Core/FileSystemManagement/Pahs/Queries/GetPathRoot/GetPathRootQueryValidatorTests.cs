#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathRoot.Fixtures;
using Lumina.Domain.Common.Errors;
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
        TestValidationResult<GetPathRootQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<GetPathRootQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<GetPathRootQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<GetPathRootQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }
}
