#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingSectionQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionQueryValidatorTests
{
    private readonly GetReadingSectionQueryValidator _validator = new();
    private readonly GetReadingSectionQueryFixture _getReadingSectionQueryFixture = new();

    [Fact]
    public void Validate_WhenBookIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create(bookId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLocationRefIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create(locationRef: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Reading.LocationRefCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Reading.LocationRefCannotBeEmpty);
    }
}
