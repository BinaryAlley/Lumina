#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;
using Lumina.Contracts.Fixtures.Core.Requests.Themes;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Themes;

/// <summary>
/// Contains unit tests for the <see cref="DeleteThemeRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeRequestMappingTests
{
    private readonly DeleteThemeRequestFixture _deleteThemeRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        DeleteThemeRequest request = _deleteThemeRequestFixture.Create();

        // Act
        DeleteThemeCommand result = request.ToCommand();

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
        DeleteThemeRequest request = new(themeId);

        // Act
        DeleteThemeCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(themeId, result.ThemeId);
    }

    [Fact]
    public void ToCommand_WhenThemeIdIsNull_ShouldMapNull()
    {
        // Arrange
        DeleteThemeRequest request = new(null);

        // Act
        DeleteThemeCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ThemeId);
    }

    [Fact]
    public void ToCommand_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<DeleteThemeRequest> requests = _deleteThemeRequestFixture.CreateMany();

        // Act
        List<DeleteThemeCommand> results = [.. requests.Select(r => r.ToCommand())];

        // Assert
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
            Assert.Equal(requests[i].ThemeId, results[i].ThemeId);
    }
}
