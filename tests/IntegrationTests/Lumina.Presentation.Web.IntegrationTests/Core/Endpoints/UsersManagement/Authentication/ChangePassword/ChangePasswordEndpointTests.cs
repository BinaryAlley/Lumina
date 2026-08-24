#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.ChangePassword;
using Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// Contains integration tests for the <c>/{culture}/auth/api-change-password</c> route served by the <see cref="ChangePasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ChangePasswordRequestFixture _changePasswordRequestFixture = new();
    private readonly ChangePasswordResponseFixture _changePasswordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ChangePasswordEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ChangePassword_WhenCalledByAuthenticatedUser_ShouldChangePasswordForCurrentUser()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ChangePasswordResponse expectedResponse = _changePasswordResponseFixture.Create(isPasswordChanged: true);
        _apiFactory.ApiClientStub.RegisterPostResponse("auth/change-password", expectedResponse);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ChangePasswordRequest request = _changePasswordRequestFixture.Create(currentPassword: "OldPass123!", newPassword: "NewPass123!", newPasswordConfirm: "NewPass123!");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateChangePasswordRequest(request, webClient.AntiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        (string Endpoint, object? Data) postRequest = _apiFactory.ApiClientStub.PostRequests.Single(postRequest => postRequest.Endpoint == "auth/change-password");
        ChangePasswordRequest sentRequest = Assert.IsType<ChangePasswordRequest>(postRequest.Data);
        Assert.Equal("testuser", sentRequest.Username);
    }

    /// <summary>
    /// Builds the change password request that sends the given <paramref name="request"/> to the change password endpoint.
    /// </summary>
    /// <param name="request">The change password request to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured change password request.</returns>
    private static HttpRequestMessage CreateChangePasswordRequest(ChangePasswordRequest request, string antiforgeryToken)
    {
        HttpRequestMessage changePasswordRequest = new(HttpMethod.Post, "/en-us/auth/api-change-password")
        {
            Content = JsonContent.Create(request)
        };
        changePasswordRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        changePasswordRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        changePasswordRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return changePasswordRequest;
    }
}
