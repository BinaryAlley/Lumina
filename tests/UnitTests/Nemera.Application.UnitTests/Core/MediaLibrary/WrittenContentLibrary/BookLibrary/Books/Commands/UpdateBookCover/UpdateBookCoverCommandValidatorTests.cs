#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCoverCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverCommandValidatorTests
{
    private readonly UpdateBookCoverCommandFixture _updateBookCoverCommandFixture = new();
    private readonly UpdateBookCoverCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(libraryId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(libraryId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenBookIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(bookId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenBookIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(bookId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenBookIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(bookId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(includeCover: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookCoverCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCoverIsPresentAndFileNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(fileName: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsMissingAndFileNameIsEmpty_ShouldNotHaveFileNameValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _updateBookCoverCommandFixture.Create(fileName: string.Empty, includeCover: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }
}
