#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.UsersManagement;
using Nemera.Presentation.Web.Core.Endpoints.Library.Statistics.GetUsers;
using Nemera.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Library.Statistics.GetUsers;

/// <summary>
/// Contains security tests for the <see cref="GetUsersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersEndpointTests : IClassFixture<NemeraWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/library/statistics/api-get-users";
    private const string EXPECTED_UPSTREAM_PATH = "auth/users";

    private readonly NemeraWebFactory _apiFactory;
    private readonly UserDtoFixture _userDtoFixture = new();
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetUsersEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetUsers_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, WEB_ROUTE);
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetUsers_WhenCalledByNonAdminUser_ShouldRedirectToAccessDeniedAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetUsers_WhenCalledByAdminUser_ShouldReturnUsersWithoutLeakingCredentials()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        UserDto[] expectedUsers = [_userDtoFixture.Create(username: "alice")];
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, expectedUsers);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(1, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.Contains(EXPECTED_UPSTREAM_PATH, _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
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
