#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Contains unit tests for the <see cref="GetBookQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookQueryValidatorTests
{
    private readonly GetBookQueryFixture _queryBookFixture = new();
    private readonly GetBookQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetBookQuery query = _queryBookFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetBookQuery query = _queryBookFixture.Create(id: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }
}
