#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemes;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Queries.GetThemes;

/// <summary>
/// Contains integration tests for the <see cref="GetThemesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetThemesEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task GetThemes_WhenCalled_ShouldReturnTheInstalledThemes()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/themes");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ThemeResponse>? themes = await response.Content.ReadFromJsonAsync<List<ThemeResponse>>(_jsonOptions);
        Assert.NotNull(themes);

        ThemeResponse? bundledTheme = themes!.FirstOrDefault(theme => theme.ThemeId == "lumina-default");
        Assert.NotNull(bundledTheme);
        Assert.NotEmpty(bundledTheme!.Name);
        Assert.Equal(ThemeInstallSource.Bundled, bundledTheme.InstallSource);
        Assert.True(bundledTheme.IsCurrent);
    }

    [Fact]
    public async Task GetThemes_WhenCalledWithoutAuthentication_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync("/api/v1/themes");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
