#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryArtworkProvidersQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryArtworkProvidersQueryValidatorTests
{
    private readonly GetLibraryArtworkProvidersQueryValidator _validator = new();
    private readonly GetLibraryArtworkProvidersQueryFixture _getLibraryArtworkProvidersQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();
        query = query with { LibraryId = Guid.Empty };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Plugins.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        GetLibraryArtworkProvidersQuery query = _getLibraryArtworkProvidersQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
