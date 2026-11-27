#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Nemera.Application.Fixtures.Core.Plugins.Queries.GetLibraryBookReaders;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Plugins.Queries.GetLibraryBookReaders;

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
