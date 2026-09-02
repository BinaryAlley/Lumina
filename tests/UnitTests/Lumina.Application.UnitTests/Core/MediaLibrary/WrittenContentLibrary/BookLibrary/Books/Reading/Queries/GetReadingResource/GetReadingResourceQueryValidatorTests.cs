#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingResourceQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceQueryValidatorTests
{
    private readonly GetReadingResourceQueryValidator _validator = new();
    private readonly GetReadingResourceQueryFixture _getReadingResourceQueryFixture = new();

    [Fact]
    public void Validate_WhenBookIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetReadingResourceQuery query = _getReadingResourceQueryFixture.Create(bookId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenResourceKeyIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetReadingResourceQuery query = _getReadingResourceQueryFixture.Create(resourceKey: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Reading.ResourceKeyCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetReadingResourceQuery query = _getReadingResourceQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Reading.ResourceKeyCannotBeEmpty);
    }
}
