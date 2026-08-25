#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetUserSettings;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Tools.Settings.GetUserSettings;

/// <summary>
/// Contains integration tests for the <c>/{culture}/tools/settings/api-get-user-settings</c> route served by the <see cref="GetUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly UserSettingsDtoFixture _userSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetUserSettingsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetUserSettings_WhenCalledByAuthenticatedUser_ShouldReturnSettingsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        UserSettingsDto expectedSettings = _userSettingsDtoFixture.Create(itemsPerPage: 24);
        _apiFactory.ApiClientStub.RegisterGetResponse("users/me/settings", expectedSettings);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/tools/settings/api-get-user-settings");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(24, json.RootElement.GetProperty("data").GetProperty("itemsPerPage").GetInt32());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "users/me/settings");
    }
}
