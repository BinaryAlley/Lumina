#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// Contains security tests for the <see cref="GetRolePermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly RolePermissionsDtoFixture _rolePermissionsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetRolePermissionsEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetRolePermissions_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/admin/api-get-permissions-by-role-id/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("auth/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetRolePermissions_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-permissions-by-role-id/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetRolePermissions_WhenCalledByAdminUser_ShouldReturnRolePermissionsData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        Guid roleId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterGetResponse($"auth/roles/{roleId}/permissions", _rolePermissionsDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-permissions-by-role-id/" + roleId);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
