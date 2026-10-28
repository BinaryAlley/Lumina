#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.Index;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Statistics.Index;

/// <summary>
/// Contains security tests for the <see cref="StatisticsIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatisticsIndexViewEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/library/statistics";

    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StatisticsIndexViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StatisticsIndexView_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync(WEB_ROUTE);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task StatisticsIndexView_WhenCalledByRegularUser_ShouldHideAdministrativeDataAndNotLeakSecrets()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("statistics-user-selector", content, StringComparison.Ordinal);
        Assert.DoesNotContain("api-get-users", content, StringComparison.Ordinal);
        Assert.DoesNotContain("auth/users", _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StatisticsIndexView_WhenCalledByAdminUser_ShouldRenderAdministrativeUserSelector()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("statistics-user-selector", content, StringComparison.Ordinal);
        Assert.Contains("api-get-users", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Releases the resources used by the test class and resets the remote API stub.
    /// </summary>
    public void Dispose()
    {
        _apiFactory.ApiClientStub.Reset();
        GC.SuppressFinalize(this);
    }
}
