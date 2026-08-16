#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksLiteQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteQueryValidatorTests
{
    private readonly GetBooksLiteQueryValidator _validator = new();
    private readonly GetBooksLiteQueryFixture _getBooksLiteQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        query = query with { Filter = query.Filter with { LibraryId = Guid.Empty } };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
