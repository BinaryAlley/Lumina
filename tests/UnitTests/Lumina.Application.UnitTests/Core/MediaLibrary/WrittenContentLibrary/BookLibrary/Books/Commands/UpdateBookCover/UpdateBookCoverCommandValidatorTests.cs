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
    private readonly UpdateBookCoverCommandFixture _commandFixture = new();
    private readonly UpdateBookCoverCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenBookIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create(bookId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create(includeCover: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookCoverCannotBeNull);
    }
}
