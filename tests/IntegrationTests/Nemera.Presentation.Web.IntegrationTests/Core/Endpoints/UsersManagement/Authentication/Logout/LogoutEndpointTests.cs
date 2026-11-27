#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Logout;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.Logout;

/// <summary>
/// Contains integration tests for the <see cref="LogoutEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LogoutEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public LogoutEndpointTests(NemeraWebFactory apiFactory)
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
