#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
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
    public void Validate_WhenIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetReadingAvailabilityQuery query = _getReadingAvailabilityQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("not-a-library-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        GetReadingAvailabilityQuery query = _getReadingAvailabilityQueryFixture.Create() with { LibraryId = libraryId };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("not-a-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenBookIdIsEmptyOrInvalid_ShouldHaveValidationError(string? bookId)
    {
        // Arrange
        GetReadingAvailabilityQuery query = _getReadingAvailabilityQueryFixture.Create() with { BookId = bookId };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.WrittenContent.BookIdCannotBeEmpty);
    }
}
