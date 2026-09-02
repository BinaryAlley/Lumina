#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Common;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetBookReaderEnabled;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.SetBookReaderEnabled;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-set-book-reader-enabled</c> route served by the <see cref="SetBookReaderEnabledEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetBookReaderEnabledEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly SetBookReaderEnabledRequestFixture _requestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetBookReaderEnabledEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SetBookReaderEnabledEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SetBookReaderEnabled_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldEnableReaderAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        SetBookReaderEnabledRequest request = _requestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);
        string expectedEndpoint = $"libraries/{libraryId}/book-readers/{pluginId}/enabled";
        _apiFactory.ApiClientStub.RegisterPutResponseFactory(expectedEndpoint, _ => new EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/libraries/manage/api-set-book-reader-enabled")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == expectedEndpoint);
    }

    [Fact]
    public async Task SetBookReaderEnabled_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        SetBookReaderEnabledRequest request = _requestFixture.Create();
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/libraries/manage/api-set-book-reader-enabled")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
