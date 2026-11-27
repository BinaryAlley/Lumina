#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Requests.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;
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

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;

/// <summary>
/// Contains integration tests for the <see cref="UpdateRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly RolePermissionsDtoFixture _rolePermissionsDtoFixture = new();
    private readonly UpdateRoleRequestFixture _updateRoleRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdateRoleEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateRole_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldForwardUpdateAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        UpdateRoleRequest request = _updateRoleRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPutResponse("auth/roles", _rolePermissionsDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/admin/api-update-role")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "auth/roles");
    }

    [Fact]
    public async Task UpdateRole_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        UpdateRoleRequest request = _updateRoleRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/admin/api-update-role")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
    }
}
