#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Nemera.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingSectionQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionQueryValidatorTests
{
    private readonly GetReadingSectionQueryValidator _validator = new();
    private readonly GetReadingSectionQueryFixture _getReadingSectionQueryFixture = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Reading.LocationRefCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("not-a-library-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create() with { LibraryId = libraryId };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("not-a-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
    public void Validate_WhenBookIdIsEmptyOrInvalid_ShouldHaveValidationError(string? bookId)
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create() with { BookId = bookId };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    public void Validate_WhenLocationRefIsEmpty_ShouldHaveValidationError(string? locationRef)
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create() with { LocationRef = locationRef };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Reading.LocationRefCannotBeEmpty);
    }
}
