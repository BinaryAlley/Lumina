#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using Lumina.Presentation.Web.Common.Requests.Common;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdateUserSettings;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Tools.Settings.UpdateUserSettings;

/// <summary>
/// Contains integration tests for the <see cref="UpdateUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly UserSettingsDtoFixture _userSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdateUserSettingsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateUserSettings_WhenCalledByAuthenticatedUser_ShouldUpdateSettings()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPutResponseFactory("users/me/settings", _ => new EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        UserSettingsDto settings = _userSettingsDtoFixture.Create(isThemeCachingEnabled: true);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateUpdateSettingsRequest(settings, webClient.AntiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("isUpdated").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "users/me/settings");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenCalledByAuthenticatedUser_ShouldForwardVuMeterSettingsToApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPutResponseFactory("users/me/settings", _ => new EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        UserSettingsDto settings = _userSettingsDtoFixture.Create(
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 20,
            vuMeterSegmentGap: 4,
            vuMeterMediumThreshold: 0.35,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1200,
            vuMeterReleasePerSecond: 7.5,
            vuMeterPeakReleasePerSecond: 0.25);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateUpdateSettingsRequest(settings, webClient.AntiforgeryToken));

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) putRequest = Assert.Single(
            _apiFactory.ApiClientStub.PutRequests,
            request => request.Endpoint == "users/me/settings");
        UserSettingsDto forwardedSettings = Assert.IsType<UserSettingsDto>(putRequest.Data);
        Assert.Equal(VuMeterOrientation.Horizontal, forwardedSettings.VuMeterOrientation);
        Assert.Equal(20, forwardedSettings.VuMeterSegmentCount);
        Assert.Equal(4, forwardedSettings.VuMeterSegmentGap);
        Assert.Equal(0.35, forwardedSettings.VuMeterMediumThreshold);
        Assert.Equal(0.9, forwardedSettings.VuMeterHighThreshold);
        Assert.False(forwardedSettings.VuMeterShowPeak);
        Assert.Equal(1200, forwardedSettings.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(7.5, forwardedSettings.VuMeterReleasePerSecond);
        Assert.Equal(0.25, forwardedSettings.VuMeterPeakReleasePerSecond);
    }

    /// <summary>
    /// Builds the update request that sends the given <paramref name="settings"/> to the user settings endpoint.
    /// </summary>
    /// <param name="settings">The user settings to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured update request.</returns>
    private static HttpRequestMessage CreateUpdateSettingsRequest(UserSettingsDto settings, string antiforgeryToken)
    {
        HttpRequestMessage updateRequest = new(HttpMethod.Post, "/en-us/tools/settings/api-update-user-settings")
        {
            Content = JsonContent.Create(settings)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return updateRequest;
    }
}
