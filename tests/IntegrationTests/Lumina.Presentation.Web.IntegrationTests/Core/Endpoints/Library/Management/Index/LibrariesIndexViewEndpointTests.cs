#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.Index;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.Index;

/// <summary>
/// Contains integration tests for the <see cref="LibrariesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibrariesIndexViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public LibrariesIndexViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task LibrariesIndexView_WhenCalledByAuthenticatedUser_ShouldRenderLibrariesManagementView()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/libraries/manage");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
    }

    [Theory]
    [InlineData("/en-us/libraries/manage", "%2Fen-us%2Flibraries%2Fmanage")] // plain page
    [InlineData("/en-us/libraries/manage?tab=scans", "%2Fen-us%2Flibraries%2Fmanage%3Ftab%3Dscans")] // query string is kept
    public async Task LibrariesIndexView_WhenNavigatedToWithoutAuthentication_ShouldRedirectToLoginWithRequestedPageAsReturnUrl(string requestedUrl, string expectedEncodedReturnUrl)
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage request = new(HttpMethod.Get, requestedUrl);
        request.Headers.Accept.ParseAdd("text/html");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"http://localhost/en-us/auth/login?returnUrl={expectedEncodedReturnUrl}", response.Headers.Location!.ToString());
    }
}
