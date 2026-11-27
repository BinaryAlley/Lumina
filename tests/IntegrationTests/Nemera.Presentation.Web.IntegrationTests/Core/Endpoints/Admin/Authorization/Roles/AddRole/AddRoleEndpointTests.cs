#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Requests.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.AddRole;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// Contains integration tests for the <see cref="AddRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly AddRoleRequestFixture _addRoleRequestFixture = new();
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly RolePermissionsDtoFixture _rolePermissionsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddRoleEndpointTests(NemeraWebFactory apiFactory)
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
