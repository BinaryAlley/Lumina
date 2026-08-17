#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.Responses.Authorization;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.GetRoles;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Authorization.Roles.GetRoles;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/api-get-roles</c> route served by the <see cref="GetRolesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetRolesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetRoles_WhenCalledWithoutAuthentication_ShouldNotExposeRoleData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        RoleDto[] unexpectedRoles = [new RoleDto(Guid.NewGuid(), "Admin")];
        _apiFactory.ApiClientStub.RegisterGetResponse("auth/roles", unexpectedRoles);
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/admin/api-get-roles");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Admin", content);
        Assert.DoesNotContain("AdminRole", content);
    }

    [Fact]
    public async Task GetRoles_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = new GetAuthorizationResponse(Guid.NewGuid(), "User", []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-roles");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetRoles_WhenCalledByAdminUser_ShouldReturnRolesData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = new GetAuthorizationResponse(Guid.NewGuid(), "Admin", []);
        RoleDto[] expectedRoles = [new RoleDto(Guid.NewGuid(), "Admin")];
        _apiFactory.ApiClientStub.RegisterGetResponse("auth/roles", expectedRoles);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/api-get-roles");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
