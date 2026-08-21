#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeSettings;
using Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeSettings;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Themes.Management.Queries.GetThemeSettings;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeSettingsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeSettingsQueryHandlerTests
{
    private readonly IThemeService _mockThemeService;
    private readonly GetThemeSettingsQueryHandler _sut;
    private readonly GetThemeSettingsQueryFixture _getThemeSettingsQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeSettingsQueryHandlerTests"/> class.
    /// </summary>
    public GetThemeSettingsQueryHandlerTests()
    {
        _mockThemeService = Substitute.For<IThemeService>();
        _sut = new GetThemeSettingsQueryHandler(_mockThemeService);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldReturnThemeSettingsFromThemeService()
    {
        // Arrange
        GetThemeSettingsQuery query = _getThemeSettingsQueryFixture.Create();
        long maxArchiveBytes = 50_000_000;
        bool allowThemeScripts = false;
        string defaultThemeId = "default-theme";
        _mockThemeService.MaxArchiveBytes.Returns(maxArchiveBytes);
        _mockThemeService.AllowThemeScripts.Returns(allowThemeScripts);
        _mockThemeService.DefaultThemeId.Returns(defaultThemeId);

        // Act
        Result<ThemeSettingsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(maxArchiveBytes, result.Value.MaxArchiveBytes);
        Assert.Equal(allowThemeScripts, result.Value.AllowThemeScripts);
        Assert.Equal(defaultThemeId, result.Value.DefaultThemeId);
    }
}
