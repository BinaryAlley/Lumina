#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksQueryValidatorTests
{
    private readonly GetBooksQueryValidator _validator = new();
    private readonly GetBooksQueryFixture _getBooksQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create(libraryId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create(libraryId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsGuidEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create(libraryId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotAValidGuid_ShouldHaveValidationError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create(libraryId: "not-a-library-guid");

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
