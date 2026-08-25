#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Contains integration tests for the <c>/{culture}/auth/api-register</c> route served by the <see cref="RegisterEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly RegisterRequestFixture _registerRequestFixture = new();
    private readonly RegisterResponseFixture _registerResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RegisterEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task Register_WhenCalledWithValidData_ShouldRegisterAccount()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        RegisterResponse expectedResponse = _registerResponseFixture.Create(username: "newuser");
        _apiFactory.ApiClientStub.RegisterPostResponse("auth/register", expectedResponse);
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        RegisterRequest request = _registerRequestFixture.Create(username: "newuser", password: "Abcd123$", passwordConfirm: "Abcd123$");

        // Act
        HttpResponseMessage response = await client.SendAsync(CreateRegisterRequest(request, antiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("newuser", json.RootElement.GetProperty("data").GetProperty("username").GetString());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/register");
    }

    [Fact]
    public async Task Register_WhenRegistrationTypeIsAdmin_ShouldCallInitializationEndpoint()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        RegisterResponse expectedResponse = _registerResponseFixture.Create(username: "admin");
        _apiFactory.ApiClientStub.RegisterPostResponse("initialization", expectedResponse);
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        RegisterRequest request = _registerRequestFixture.Create(username: "admin", password: "Abcd123$", passwordConfirm: "Abcd123$", registrationType: "Admin");

        // Act
        HttpResponseMessage response = await client.SendAsync(CreateRegisterRequest(request, antiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "initialization");
    }

    /// <summary>
    /// Builds the register request that sends the given <paramref name="request"/> to the register endpoint.
    /// </summary>
    /// <param name="request">The register request to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured register request.</returns>
    private static HttpRequestMessage CreateRegisterRequest(RegisterRequest request, string antiforgeryToken)
    {
        HttpRequestMessage registerRequest = new(HttpMethod.Post, "/en-us/auth/api-register")
        {
            Content = JsonContent.Create(request)
        };
        registerRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        registerRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        registerRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return registerRequest;
    }
}
