#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdateUserSettings;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Tools.Settings.UpdateUserSettings;

/// <summary>
/// Contains security tests for the <c>/{culture}/tools/settings/api-update-user-settings</c> route served by the <see cref="UpdateUserSettingsEndpoint"/> class.
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
    public async Task UpdateUserSettings_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage updateRequest = new(HttpMethod.Post, "/en-us/tools/settings/api-update-user-settings")
        {
            Content = JsonContent.Create(_userSettingsDtoFixture.Create())
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "users/me/settings");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Post, "/en-us/tools/settings/api-update-user-settings")
        {
            Content = JsonContent.Create(_userSettingsDtoFixture.Create())
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "users/me/settings");
    }
}
