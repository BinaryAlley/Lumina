#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserPermissions;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserPermissions;

/// <summary>
/// Contains security tests for the <see cref="GetUserPermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly PermissionDtoFixture _permissionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetUserPermissionsEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetUserPermissions_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/admin/api-get-permissions-by-user-id/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("auth/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetUserPermissions_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-permissions-by-user-id/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetUserPermissions_WhenCalledByAdminUser_ShouldReturnUserPermissionsData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        Guid userId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterGetResponse($"auth/users/{userId}/permissions", new PermissionDto[] { _permissionDtoFixture.Create() });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-permissions-by-user-id/" + userId);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
