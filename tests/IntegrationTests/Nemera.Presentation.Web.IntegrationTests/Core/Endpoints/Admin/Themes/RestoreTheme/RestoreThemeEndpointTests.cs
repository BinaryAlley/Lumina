#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes.RestoreTheme;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes.RestoreTheme;

/// <summary>
/// Contains integration tests for the <see cref="RestoreThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RestoreThemeEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RestoreTheme_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldRestoreThemeAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string THEME_ID = "test-theme";
        _apiFactory.ApiClientStub.RegisterPostSuccess($"themes/{THEME_ID}/restore");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage restoreRequest = new(HttpMethod.Post, $"/en-us/admin/themes/api-restore-theme/{THEME_ID}")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        restoreRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        restoreRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        restoreRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == $"themes/{THEME_ID}/restore");
    }
}
