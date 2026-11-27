#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Themes;
using Nemera.Application.Core.Themes.Management.Commands.RestoreTheme;
using Nemera.Contracts.Fixtures.Core.Requests.Themes;
using Nemera.Contracts.Requests.Themes;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Themes;

/// <summary>
/// Contains unit tests for the <see cref="RestoreThemeRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeRequestMappingTests
{
    private readonly RestoreThemeRequestFixture _restoreThemeRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingRequest_ShouldMapThemeId()
    {
        // Arrange
        RestoreThemeRequest request = _restoreThemeRequestFixture.Create(themeId: "editorial-paper");

        // Act
        RestoreThemeCommand command = request.ToCommand();

        // Assert
        Assert.NotNull(command);
        Assert.Equal(request.ThemeId, command.ThemeId);
    }
}
