#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeTemplate;
using Lumina.Contracts.Fixtures.Core.Requests.Themes;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Themes;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeTemplateRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateRequestMappingTests
{
    private readonly GetThemeTemplateRequestFixture _getThemeTemplateRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetThemeTemplateRequest request = _getThemeTemplateRequestFixture.Create();

        // Act
        GetThemeTemplateQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ThemeId, result.ThemeId);
        Assert.Equal(request.PageKey, result.PageKey);
    }

    [Theory]
    [InlineData("default-theme", "home")]
    [InlineData("my-theme", "library")]
    [InlineData("T-123", "settings")]
    public void ToQuery_WhenMappingDifferentThemeIdsAndPageKeys_ShouldMapCorrectly(string themeId, string pageKey)
    {
        // Arrange
        GetThemeTemplateRequest request = new(themeId, pageKey);

        // Act
        GetThemeTemplateQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(themeId, result.ThemeId);
        Assert.Equal(pageKey, result.PageKey);
    }

    [Fact]
    public void ToQuery_WhenFieldsAreNull_ShouldMapNull()
    {
        // Arrange
        GetThemeTemplateRequest request = new(null, null);

        // Act
        GetThemeTemplateQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ThemeId);
        Assert.Null(result.PageKey);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GetThemeTemplateRequest> requests = _getThemeTemplateRequestFixture.CreateMany();

        // Act
        List<GetThemeTemplateQuery> results = [.. requests.Select(r => r.ToQuery())];

        // Assert
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].ThemeId, results[i].ThemeId);
            Assert.Equal(requests[i].PageKey, results[i].PageKey);
        }
    }
}
