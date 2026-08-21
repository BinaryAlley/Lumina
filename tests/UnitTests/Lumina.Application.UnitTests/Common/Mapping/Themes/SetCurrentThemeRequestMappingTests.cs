#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using Lumina.Contracts.Fixtures.Core.Requests.Themes;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Themes;

/// <summary>
/// Contains unit tests for the <see cref="SetCurrentThemeRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeRequestMappingTests
{
    private readonly SetCurrentThemeRequestFixture _setCurrentThemeRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        SetCurrentThemeRequest request = _setCurrentThemeRequestFixture.Create();

        // Act
        SetCurrentThemeCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ThemeId, result.ThemeId);
    }

    [Theory]
    [InlineData("default-theme")]
    [InlineData("my-uploaded-theme")]
    [InlineData("T-123")]
    public void ToCommand_WhenMappingDifferentThemeIds_ShouldMapCorrectly(string themeId)
    {
        // Arrange
        SetCurrentThemeRequest request = new(themeId);

        // Act
        SetCurrentThemeCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(themeId, result.ThemeId);
    }

    [Fact]
    public void ToCommand_WhenThemeIdIsNull_ShouldMapNull()
    {
        // Arrange
        SetCurrentThemeRequest request = new(null);

        // Act
        SetCurrentThemeCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ThemeId);
    }

    [Fact]
    public void ToCommand_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<SetCurrentThemeRequest> requests = _setCurrentThemeRequestFixture.CreateMany();

        // Act
        List<SetCurrentThemeCommand> results = [.. requests.Select(r => r.ToCommand())];

        // Assert
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
            Assert.Equal(requests[i].ThemeId, results[i].ThemeId);
    }
}
