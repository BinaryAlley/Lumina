#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeArtworkProvidersQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersQueryValidatorTests
{
    private readonly GetLibraryTypeArtworkProvidersQueryValidator _validator = new();
    private readonly GetLibraryTypeArtworkProvidersQueryFixture _getLibraryTypeArtworkProvidersQueryFixture = new();

    [Fact]
    public void Validate_WhenLibraryTypeIsNull_ShouldHaveValidationError()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create() with { LibraryType = null };

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryTypeCannotBeNull);
    }

    [Theory]
    [InlineData("UnknownLibraryType")] // Not a library type.
    [InlineData("999")] // A number that is not a defined library type.
    [InlineData("0")] // The numeric value of a defined library type, which is not a member name.
    [InlineData("")] // Empty.
    public void Validate_WhenLibraryTypeIsUnknown_ShouldHaveValidationError(string libraryType)
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create(libraryType: libraryType);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.UnknownLibraryType);
    }

    [Theory]
    [InlineData("Book")]
    [InlineData("music")] // The casing of the type is ignored.
    public void Validate_WhenLibraryTypeIsValid_ShouldNotHaveValidationError(string libraryType)
    {
        // Arrange
        GetLibraryTypeArtworkProvidersQuery query = _getLibraryTypeArtworkProvidersQueryFixture.Create(libraryType: libraryType);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryTypeCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Library.UnknownLibraryType);
    }
}
