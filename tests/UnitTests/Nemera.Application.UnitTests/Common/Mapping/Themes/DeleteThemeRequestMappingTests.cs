#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Themes;
using Nemera.Application.Core.Themes.Management.Commands.DeleteTheme;
using Nemera.Contracts.Fixtures.Core.Requests.Themes;
using Nemera.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Themes;

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
        DeleteThemeRequest request = _deleteThemeRequestFixture.Create(themeId);

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
        DeleteThemeRequest request = _deleteThemeRequestFixture.Create(includeThemeId: false);

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
