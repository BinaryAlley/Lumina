#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;
using Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Contains integration tests for the <c>/{culture}/auth/api-recover-password</c> route served by the <see cref="RecoverPasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly RecoverPasswordRequestFixture _recoverPasswordRequestFixture = new();
    private readonly RecoverPasswordResponseFixture _recoverPasswordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RecoverPasswordEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RecoverPassword_WhenCalledWithValidData_ShouldResetPassword()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        RecoverPasswordResponse expectedResponse = _recoverPasswordResponseFixture.Create(isPasswordReset: true);
        _apiFactory.ApiClientStub.RegisterPostResponse("auth/recover-password", expectedResponse);
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create(username: "testuser", totpCode: "123456");

        // Act
        HttpResponseMessage response = await client.SendAsync(CreateRecoverPasswordRequest(request, antiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("isPasswordReset").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/recover-password");
    }

    /// <summary>
    /// Builds the recover password request that sends the given <paramref name="request"/> to the recover password endpoint.
    /// </summary>
    /// <param name="request">The recover password request to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured recover password request.</returns>
    private static HttpRequestMessage CreateRecoverPasswordRequest(RecoverPasswordRequest request, string antiforgeryToken)
    {
        HttpRequestMessage recoverPasswordRequest = new(HttpMethod.Post, "/en-us/auth/api-recover-password")
        {
            Content = JsonContent.Create(request)
        };
        recoverPasswordRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        recoverPasswordRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        recoverPasswordRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return recoverPasswordRequest;
    }
}
