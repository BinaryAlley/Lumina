#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Authorization;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.AddRole;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/api-create-role</c> route served by the <see cref="AddRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly AddRoleRequestFixture _addRoleRequestFixture = new();
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly RolePermissionsDtoFixture _rolePermissionsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddRoleEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AddRole_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldForwardRoleAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        AddRoleRequest request = _addRoleRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostResponse("auth/roles", _rolePermissionsDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/admin/api-create-role")
        {
            Content = JsonContent.Create(request)
        };
        createRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        createRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/roles");
    }

    [Fact]
    public async Task AddRole_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AddRoleRequest request = _addRoleRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/admin/api-create-role")
        {
            Content = JsonContent.Create(request)
        };
        createRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        createRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
    }
}
