#region ========================================================================= USING =====================================================================================
using FluentValidation.TestHelper;
using Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Domain.Common.Errors;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Contains unit tests for the <see cref="GetThumbnailQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailQueryValidatorTests
{
    private readonly GetThumbnailQueryValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailQueryValidatorTests"/> class.
    /// </summary>
    public GetThumbnailQueryValidatorTests()
    {
        _validator = new GetThumbnailQueryValidator();
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetThumbnailQuery query = new(null!, 1);

        // Act
        TestValidationResult<GetThumbnailQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path).WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetThumbnailQuery query = new(string.Empty, 1);

        // Act
        TestValidationResult<GetThumbnailQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Path).WithErrorMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }

    [Fact]
    public void Validate_WhenPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetThumbnailQuery query = new("test", 1);

        // Act
        TestValidationResult<GetThumbnailQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Path);
    }

    [Fact]
    public void Validate_WhenQualityIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        GetThumbnailQuery query = new("test", -1);

        // Act
        TestValidationResult<GetThumbnailQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quality).WithErrorMessage(Errors.Thumbnails.ImageQaulityMustBeBetweenZeroAndOneHundred.Code);
    }

    [Fact]
    public void Validate_WhenQualityIsOver100_ShouldHaveValidationError()
    {
        // Arrange
        GetThumbnailQuery query = new("test", 101);

        // Act
        TestValidationResult<GetThumbnailQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quality).WithErrorMessage(Errors.Thumbnails.ImageQaulityMustBeBetweenZeroAndOneHundred.Code);
    }

    [Fact]
    public void Validate_WhenQualityIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetThumbnailQuery query = new("test", 1);

        // Act
        TestValidationResult<GetThumbnailQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quality);
    }

}
