#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryBookReaders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryBookReadersQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersQueryValidatorTests
{
    private readonly GetLibraryBookReadersQueryValidator _validator = new();
    private readonly GetLibraryBookReadersQueryFixture _getLibraryBookReadersQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create(libraryId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetLibraryBookReadersQuery query = _getLibraryBookReadersQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }
}
