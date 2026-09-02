#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingAvailabilityQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityQueryValidatorTests
{
    private readonly GetReadingAvailabilityQueryValidator _validator = new();
    private readonly GetReadingAvailabilityQueryFixture _getReadingAvailabilityQueryFixture = new();

    [Fact]
    public void Validate_WhenBookIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetReadingAvailabilityQuery query = _getReadingAvailabilityQueryFixture.Create(bookId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenBookIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetReadingAvailabilityQuery query = _getReadingAvailabilityQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
    }
}
