#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;
using Lumina.Contracts.Fixtures.Core.Requests.Themes;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Themes;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeArchiveRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveRequestMappingTests
{
    private readonly GetThemeArchiveRequestFixture _getThemeArchiveRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetThemeArchiveRequest request = _getThemeArchiveRequestFixture.Create();

        // Act
        GetThemeArchiveQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ThemeId, result.ThemeId);
    }

    [Theory]
    [InlineData("default-theme")]
    [InlineData("my-uploaded-theme")]
    [InlineData("T-123")]
    public void ToQuery_WhenMappingDifferentThemeIds_ShouldMapCorrectly(string themeId)
    {
        // Arrange
        GetThemeArchiveRequest request = new(themeId);

        // Act
        GetThemeArchiveQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(themeId, result.ThemeId);
    }

    [Fact]
    public void ToQuery_WhenThemeIdIsNull_ShouldMapNull()
    {
        // Arrange
        GetThemeArchiveRequest request = new(null);

        // Act
        GetThemeArchiveQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ThemeId);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GetThemeArchiveRequest> requests = _getThemeArchiveRequestFixture.CreateMany();

        // Act
        List<GetThemeArchiveQuery> results = [.. requests.Select(r => r.ToQuery())];

        // Assert
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
            Assert.Equal(requests[i].ThemeId, results[i].ThemeId);
    }
}
