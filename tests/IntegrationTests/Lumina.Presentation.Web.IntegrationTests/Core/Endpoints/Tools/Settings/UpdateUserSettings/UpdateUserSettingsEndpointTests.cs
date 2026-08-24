#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
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
/// Contains integration tests for the <c>/{culture}/tools/settings/api-update-user-settings</c> route served by the <see cref="UpdateUserSettingsEndpoint"/> class.
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
