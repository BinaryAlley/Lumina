#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using Nemera.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Nemera.Contracts.Requests.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryPathTemplatePartsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryPathTemplatePartsRequestMappingTests
{
    private readonly GetLibraryPathTemplatePartsRequestFixture _getLibraryPathTemplatePartsRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapLibraryType()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _getLibraryPathTemplatePartsRequestFixture.Create(libraryType: "Book");

        // Act
        GetLibraryPathTemplatePartsQuery result = request.ToQuery();

        // Assert
        Assert.Equal("Book", result.LibraryType);
    }

    [Theory]
    [InlineData("Book")] // A written content library type.
    [InlineData("Movie")] // A video library type.
    [InlineData("Music")] // An audio library type.
    public void ToQuery_WhenMappingDifferentLibraryTypes_ShouldMapLibraryType(string libraryType)
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _getLibraryPathTemplatePartsRequestFixture.Create(libraryType: libraryType);

        // Act
        GetLibraryPathTemplatePartsQuery result = request.ToQuery();

        // Assert
        Assert.Equal(libraryType, result.LibraryType);
    }
}
