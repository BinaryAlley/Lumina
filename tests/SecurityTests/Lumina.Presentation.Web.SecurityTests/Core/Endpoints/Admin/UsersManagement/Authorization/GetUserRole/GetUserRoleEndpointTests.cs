#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserRole;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/api-get-role-by-user-id/{userId}</c> route served by the <see cref="GetUserRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly RoleDtoFixture _roleDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetUserRoleEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetUserRole_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/admin/api-get-role-by-user-id/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("auth/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetUserRole_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-role-by-user-id/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetUserRole_WhenCalledByAdminUser_ShouldReturnUserRoleData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        Guid userId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterGetResponse($"auth/users/{userId}/role", _roleDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-role-by-user-id/" + userId);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
