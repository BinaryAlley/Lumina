#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.Admin.ManagePermissions;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.ManagePermissions;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/manage-permissions</c> route served by the <see cref="ManagePermissionsViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ManagePermissionsViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly PermissionDtoFixture _permissionDtoFixture = new();
    private readonly RoleDtoFixture _roleDtoFixture = new();
    private readonly UserDtoFixture _userDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagePermissionsViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ManagePermissionsViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ManagePermissions_WhenCalledByAuthenticatedAdmin_ShouldRenderPermissionsManagementPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        UserDto[] expectedUsers = [.. _userDtoFixture.CreateMany()];
        RoleDto[] expectedRoles = [.. _roleDtoFixture.CreateMany()];
        PermissionDto[] expectedPermissions = [.. _permissionDtoFixture.CreateMany()];
        _apiFactory.ApiClientStub.RegisterGetResponse("auth/users", expectedUsers);
        _apiFactory.ApiClientStub.RegisterGetResponse("auth/roles", expectedRoles);
        _apiFactory.ApiClientStub.RegisterGetResponse("auth/permissions", expectedPermissions);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/manage-permissions");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task ManagePermissions_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/manage-permissions");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
    }
}
