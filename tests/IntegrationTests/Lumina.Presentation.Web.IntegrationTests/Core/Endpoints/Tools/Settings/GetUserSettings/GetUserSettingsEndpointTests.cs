#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
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
/// Contains integration tests for the <see cref="GetUserSettingsEndpoint"/> class.
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

    [Fact]
    public async Task GetUserSettings_WhenCalledByAuthenticatedUser_ShouldReturnVuMeterSettingsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        UserSettingsDto expectedSettings = _userSettingsDtoFixture.Create(
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 20,
            vuMeterSegmentGap: 4,
            vuMeterMediumThreshold: 0.35,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1200,
            vuMeterReleasePerSecond: 7.5,
            vuMeterPeakReleasePerSecond: 0.25);
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
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal("Horizontal", data.GetProperty("vuMeterOrientation").GetString());
        Assert.Equal(20, data.GetProperty("vuMeterSegmentCount").GetInt32());
        Assert.Equal(4, data.GetProperty("vuMeterSegmentGap").GetInt32());
        Assert.Equal(0.35, data.GetProperty("vuMeterMediumThreshold").GetDouble());
        Assert.Equal(0.9, data.GetProperty("vuMeterHighThreshold").GetDouble());
        Assert.False(data.GetProperty("vuMeterShowPeak").GetBoolean());
        Assert.Equal(1200, data.GetProperty("vuMeterPeakHoldDurationInMilliseconds").GetInt32());
        Assert.Equal(7.5, data.GetProperty("vuMeterReleasePerSecond").GetDouble());
        Assert.Equal(0.25, data.GetProperty("vuMeterPeakReleasePerSecond").GetDouble());
    }
}
