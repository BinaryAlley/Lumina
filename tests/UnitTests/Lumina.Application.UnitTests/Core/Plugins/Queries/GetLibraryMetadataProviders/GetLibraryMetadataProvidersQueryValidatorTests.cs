#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryMetadataProvidersQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryMetadataProvidersQueryValidatorTests
{
    private readonly GetLibraryMetadataProvidersQueryValidator _validator = new();
    private readonly GetLibraryMetadataProvidersQueryFixture _getLibraryMetadataProvidersQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        GetLibraryMetadataProvidersQuery query = _getLibraryMetadataProvidersQueryFixture.Create();
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
        GetLibraryMetadataProvidersQuery query = _getLibraryMetadataProvidersQueryFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
