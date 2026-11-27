#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Nemera.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Contains unit tests for the <see cref="GetBookQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookQueryValidatorTests
{
    private readonly GetBookQueryFixture _getBookQueryFixture = new();
    private readonly GetBookQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("not-a-library-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create() with { LibraryId = libraryId };

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
        GetBookQuery query = _getBookQueryFixture.Create() with { BookId = bookId };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }
}
