#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/api-get-permissions-by-role-id/{roleId}</c> route served by the <see cref="GetRolePermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly RolePermissionsDtoFixture _rolePermissionsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetRolePermissionsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetRolePermissions_WhenCalledByAuthenticatedAdmin_ShouldReturnRolePermissionsJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        Guid roleId = Guid.NewGuid();
        RolePermissionsDto expectedResponse = _rolePermissionsDtoFixture.Create();
        _apiFactory.ApiClientStub.RegisterGetResponse($"auth/roles/{roleId}/permissions", expectedResponse);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/admin/api-get-permissions-by-role-id/{roleId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedResponse.Permissions.Length, json.RootElement.GetProperty("data").GetProperty("permissions").GetArrayLength());
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
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
    }
}
