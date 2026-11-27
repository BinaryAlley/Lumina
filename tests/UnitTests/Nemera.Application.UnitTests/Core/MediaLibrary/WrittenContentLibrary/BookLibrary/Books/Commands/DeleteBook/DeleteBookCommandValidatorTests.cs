#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;
using Nemera.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;

/// <summary>
/// Contains unit tests for the <see cref="DeleteBookCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteBookCommandValidatorTests
{
    private readonly DeleteBookCommandFixture _deleteBookCommandFixture = new();
    private readonly DeleteBookCommandValidator _validator = new();

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value, which both the not empty rule and the format rule reject.
    [InlineData("not-a-library-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // Missing route value.
    [InlineData("")] // Empty route value.
    [InlineData("   ")] // Whitespace route value, which both the not empty rule and the format rule reject.
    [InlineData("not-a-book-guid")] // Non-Guid route value.
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty Guid route value.
    public void Validate_WhenBookIdIsEmptyOrInvalid_ShouldHaveValidationError(string? bookId)
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create(bookId: bookId, includeBookId: bookId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenBookIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create(bookId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
