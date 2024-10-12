#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathParent;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathParent.Fixtures;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathParent;

/// <summary>
/// Contains unit tests for the <see cref="GetPathParentQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentQueryValidatorTests
{
    private readonly GetPathParentQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentQueryValidatorTests"/> class.
    /// </summary>
    public GetPathParentQueryValidatorTests()
    {
        _validator = new GetPathParentQueryValidator();
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetPathParentQuery query = GetPathParentQueryFixture.CreateGetPathParentQuery();
        query = query with { Path = null! };

        // Act
        TestValidationResult<GetPathParentQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetPathParentQuery query = GetPathParentQueryFixture.CreateGetPathParentQuery();
        query = query with { Path = string.Empty };

        // Act
        TestValidationResult<GetPathParentQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetPathParentQuery query = GetPathParentQueryFixture.CreateGetPathParentQuery();
        query = query with { Path = "   " };

        // Act
        TestValidationResult<GetPathParentQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path)
            .WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetPathParentQuery query = GetPathParentQueryFixture.CreateGetPathParentQuery();
        query = query with { Path = "/valid/path" };

        // Act
        TestValidationResult<GetPathParentQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }
}
