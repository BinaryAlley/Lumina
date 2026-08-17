#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Login;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Contains security tests for the <c>/{culture}/auth/api-login</c> route served by the <see cref="LoginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public LoginEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("admin'--")] // comment injection
    [InlineData("' UNION SELECT * FROM Users--")] // union injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    [InlineData("{{7*7}}")] // template injection
    public async Task Login_WithInjectionAttemptInUsername_ShouldRemainSecure(string maliciousUsername)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/login", new ApiException(new ProblemDetailsDto { Title = "General.Failure", Detail = "InvalidUsernameOrPassword" }, HttpStatusCode.Forbidden, "auth/login"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage loginRequest = CreateLoginRequest(new LoginRequest(Username: maliciousUsername, Password: "Abcd123$"), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(loginRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content);
        Assert.DoesNotContain("Abcd123$", content);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldNotExposeSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage loginRequest = CreateLoginRequest(new LoginRequest(Username: "testuser", Password: "TestPass123!"), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(loginRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("TestPass123!", content);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        // the JWT token returned by the API must not be exposed in plaintext; it is stored encrypted in the Token cookie
        Assert.DoesNotContain("test_jwt_token", content);
        string allSetCookies = string.Join("; ", response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("Token=", allSetCookies);
        Assert.DoesNotContain("test_jwt_token", allSetCookies);
    }

    [Fact]
    public async Task Login_WhenCalled_ShouldSetAuthenticationCookieWithSecurityAttributes()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage loginRequest = CreateLoginRequest(new LoginRequest(Username: "testuser", Password: "TestPass123!"), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        string authCookie = response.Headers.GetValues("Set-Cookie").First(cookie => cookie.StartsWith(".Lumina.Auth", StringComparison.Ordinal));
        Assert.Contains("httponly", authCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", authCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", authCookie, StringComparison.OrdinalIgnoreCase);
        string tokenCookie = response.Headers.GetValues("Set-Cookie").First(cookie => cookie.StartsWith("Token=", StringComparison.Ordinal) && !cookie.Contains("1970"));
        Assert.Contains("httponly", tokenCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", tokenCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", tokenCookie, StringComparison.OrdinalIgnoreCase);
    }

    private static HttpRequestMessage CreateLoginRequest(LoginRequest request, string antiforgeryToken)
    {
        HttpRequestMessage loginRequest = new(HttpMethod.Post, "/en-us/auth/api-login")
        {
            Content = JsonContent.Create(request)
        };
        loginRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        loginRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        loginRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return loginRequest;
    }
}
