#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetBookReaderEnabled;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.SetBookReaderEnabled;

/// <summary>
/// Contains security tests for the <c>/{culture}/libraries/manage/api-set-book-reader-enabled</c> route served by the <see cref="SetBookReaderEnabledEndpoint"/> class.
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
    public async Task SetBookReaderEnabled_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        SetBookReaderEnabledRequest request = _requestFixture.Create();
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/libraries/manage/api-set-book-reader-enabled")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task SetBookReaderEnabled_WhenCalledWithoutAntiforgeryToken_ShouldReturnBadRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        SetBookReaderEnabledRequest request = _requestFixture.Create();
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/libraries/manage/api-set-book-reader-enabled")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }
}
