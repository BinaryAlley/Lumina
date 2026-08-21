#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;
using Lumina.Contracts.Fixtures.Core.Requests.Themes;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Themes;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeAssetRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetRequestMappingTests
{
    private readonly GetThemeAssetRequestFixture _getThemeAssetRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetThemeAssetRequest request = _getThemeAssetRequestFixture.Create();

        // Act
        GetThemeAssetQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ThemeId, result.ThemeId);
        Assert.Equal(request.AssetPath, result.AssetPath);
    }

    [Theory]
    [InlineData("default-theme", "assets/logo.png")]
    [InlineData("my-theme", "css/main.css")]
    [InlineData("T-123", "js/app.js")]
    public void ToQuery_WhenMappingDifferentThemeIdsAndAssetPaths_ShouldMapCorrectly(string themeId, string assetPath)
    {
        // Arrange
        GetThemeAssetRequest request = new(themeId, assetPath);

        // Act
        GetThemeAssetQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(themeId, result.ThemeId);
        Assert.Equal(assetPath, result.AssetPath);
    }

    [Fact]
    public void ToQuery_WhenFieldsAreNull_ShouldMapNull()
    {
        // Arrange
        GetThemeAssetRequest request = new(null, null);

        // Act
        GetThemeAssetQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ThemeId);
        Assert.Null(result.AssetPath);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GetThemeAssetRequest> requests = _getThemeAssetRequestFixture.CreateMany();

        // Act
        List<GetThemeAssetQuery> results = [.. requests.Select(r => r.ToQuery())];

        // Assert
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].ThemeId, results[i].ThemeId);
            Assert.Equal(requests[i].AssetPath, results[i].AssetPath);
        }
    }
}
