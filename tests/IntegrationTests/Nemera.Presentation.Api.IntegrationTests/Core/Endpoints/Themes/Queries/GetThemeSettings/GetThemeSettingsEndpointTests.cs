#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeSettings;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Queries.GetThemeSettings;

/// <summary>
/// Contains integration tests for the <see cref="GetThemeSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeSettingsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetThemeSettingsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetThemeSettings_WhenCalled_ShouldReturnTheThemeEngineSettings()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/themes/settings");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ThemeSettingsResponse? settings = await response.Content.ReadFromJsonAsync<ThemeSettingsResponse>(_jsonOptions);
        Assert.NotNull(settings);
        Assert.Equal(8 * 1024 * 1024, settings!.MaxArchiveBytes);
        Assert.Equal("lumina-default", settings.DefaultThemeId);
    }

    [Fact]
    public async Task GetThemeSettings_WhenCalledWithoutAuthentication_ShouldReturnTheThemeEngineSettings()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync("/api/v1/themes/settings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
