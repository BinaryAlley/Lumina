#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Contains security tests for the <c>/{culture}/auth/api-register</c> route served by the <see cref="RegisterEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly RegisterRequestFixture _registerRequestFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RegisterEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("admin'--")] // comment injection
    [InlineData("' UNION SELECT * FROM Users--")] // union injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    [InlineData("{{7*7}}")] // template injection
    public async Task Register_WithInjectionAttemptInUsername_ShouldRemainSecure(string maliciousUsername)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/register", new ApiException(_problemDetailsDtoFixture.Create(title: "General.Failure", detail: "RegistrationFailed"), HttpStatusCode.Forbidden, "auth/register"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        RegisterRequest request = _registerRequestFixture.Create(username: maliciousUsername, password: "Abcd123$", passwordConfirm: "Abcd123$");

        // Act
        HttpResponseMessage response = await client.SendAsync(CreateRegisterRequest(request, antiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        RegisterRequest request = _registerRequestFixture.Create(username: "newuser", password: "Abcd123$", passwordConfirm: "Abcd123$");
        HttpRequestMessage registerRequest = new(HttpMethod.Post, "/en-us/auth/api-register")
        {
            Content = JsonContent.Create(request)
        };
        registerRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/register");
    }

    [Fact]
    public async Task Register_WhenApiReturnsError_ShouldReturnCleanError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/register", new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Failure", detail: "RegistrationFailed"),
            HttpStatusCode.Conflict));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        RegisterRequest request = _registerRequestFixture.Create(username: "newuser", password: "Abcd123$", passwordConfirm: "Abcd123$");

        // Act
        HttpResponseMessage response = await client.SendAsync(CreateRegisterRequest(request, antiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
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
