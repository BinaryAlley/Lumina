#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Requests.Themes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.SetCurrentTheme;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes.SetCurrentTheme;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/themes/api-set-current-theme</c> route served by the <see cref="SetCurrentThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SetCurrentThemeEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SetCurrentTheme_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldSetCurrentThemeAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string THEME_ID = "test-theme";
        _apiFactory.ApiClientStub.RegisterPutResponse("themes/current", _themeResponseDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage setCurrentThemeRequest = new(HttpMethod.Put, "/en-us/admin/themes/api-set-current-theme")
        {
            Content = JsonContent.Create(new SetCurrentThemeRequest(THEME_ID))
        };
        setCurrentThemeRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        setCurrentThemeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        setCurrentThemeRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(setCurrentThemeRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "themes/current");
    }
}
