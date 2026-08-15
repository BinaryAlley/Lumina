#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibrary;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetLibrary;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryQueryValidatorTests
{
    private readonly GetLibraryQueryValidator _validator = new();
    private readonly GetLibraryQueryFixture _getLibraryQueryFixture = new();

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetLibraryQuery query = _getLibraryQueryFixture.Create();
        query = query with { Id = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetLibraryQuery query = _getLibraryQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
