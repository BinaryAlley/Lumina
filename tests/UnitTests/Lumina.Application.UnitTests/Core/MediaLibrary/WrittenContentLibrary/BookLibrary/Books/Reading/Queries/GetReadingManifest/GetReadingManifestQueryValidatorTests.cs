#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingManifestQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestQueryValidatorTests
{
    private readonly GetReadingManifestQueryValidator _validator = new();
    private readonly GetReadingManifestQueryFixture _getReadingManifestQueryFixture = new();

    [Fact]
    public void Validate_WhenBookIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create(bookId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenBookIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Reading.BookIdCannotBeEmpty);
    }
}
