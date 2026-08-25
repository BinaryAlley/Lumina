#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Authorization;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.AddRole;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/api-create-role</c> route served by the <see cref="AddRoleEndpoint"/> class.
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
    public async Task AddRole_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/admin/api-create-role")
        {
            Content = JsonContent.Create(_addRoleRequestFixture.Create())
        };
        createRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        createRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/roles");
    }

    [Fact]
    public async Task AddRole_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/admin/api-create-role")
        {
            Content = JsonContent.Create(_addRoleRequestFixture.Create())
        };
        createRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/roles");
    }

    [Fact]
    public async Task AddRole_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/admin/api-create-role")
        {
            Content = JsonContent.Create(_addRoleRequestFixture.Create())
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

    [Theory]
    [InlineData("'; DROP TABLE Roles--")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddRole_WithInjectionInRoleName_ShouldRemainSecure(string maliciousRoleName)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        AddRoleRequest request = _addRoleRequestFixture.Create(roleName: maliciousRoleName);
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }
}
