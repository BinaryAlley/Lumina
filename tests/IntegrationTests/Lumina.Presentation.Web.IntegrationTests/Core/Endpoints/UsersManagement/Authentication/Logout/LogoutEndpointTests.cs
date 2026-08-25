#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Logout;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.Logout;

/// <summary>
/// Contains integration tests for the <c>/{culture}/auth/logout</c> route served by the <see cref="LogoutEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LogoutEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public LogoutEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task Logout_WhenCalledByAuthenticatedUser_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/auth/logout");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/en-us/auth/login", response.Headers.Location!.ToString());
    }
}
