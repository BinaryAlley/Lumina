#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

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
    [InlineData("Book")] // a written content library type
    [InlineData("Movie")] // a video library type
    [InlineData("Music")] // an audio library type
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
