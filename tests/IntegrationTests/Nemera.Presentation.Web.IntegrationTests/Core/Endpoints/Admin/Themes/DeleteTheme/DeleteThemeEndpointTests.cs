#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes.DeleteTheme;
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

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes.DeleteTheme;

/// <summary>
/// Contains integration tests for the <see cref="DeleteThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeleteThemeEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DeleteTheme_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldDeleteThemeAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string THEME_ID = "test-theme";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess($"themes/{THEME_ID}");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/admin/themes/api-delete-theme/{THEME_ID}")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        deleteRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.DeleteEndpointsCalled, endpoint => endpoint == $"themes/{THEME_ID}");
    }

    [Fact]
    public async Task DeleteTheme_WhenCalledWithEmptyThemeId_ShouldReturnBadRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, "/en-us/admin/themes/api-delete-theme/%20")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        deleteRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }
}
