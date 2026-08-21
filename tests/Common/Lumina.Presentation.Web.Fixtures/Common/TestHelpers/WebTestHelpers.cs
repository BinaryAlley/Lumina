#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.TestHelpers;

/// <summary>
/// Represents an HTTP client that has been authenticated through the real login flow of the Web application.
/// </summary>
/// <param name="Client">The authenticated HTTP client.</param>
/// <param name="AntiforgeryToken">The antiforgery token captured after the login, valid for subsequent mutating requests made by the authenticated user.</param>
[ExcludeFromCodeCoverage]
public record AuthenticatedWebClient(HttpClient Client, string AntiforgeryToken);

/// <summary>
/// Helper methods for exercising the authentication and antiforgery flows of the Web application in integration and security tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class WebTestHelpers
{
    private const string LOGIN_PAGE_PATH = "/en-us/auth/login";
    private const string LOGIN_ENDPOINT_PATH = "/en-us/auth/api-login";
    // the change password page renders an antiforgery token for the authenticated user without calling the remote API
    private const string AUTHENTICATED_TOKEN_PAGE_PATH = "/en-us/auth/change-password";

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]*)\"")]
    private static partial Regex AntiforgeryTokenRegex();

    /// <summary>
    /// Creates an anonymous HTTP client that does not follow redirects.
    /// </summary>
    /// <param name="factory">The Web application factory.</param>
    /// <returns>An anonymous HTTP client.</returns>
    public static HttpClient CreateAnonymousClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    /// <summary>
    /// Creates an HTTP client that has been authenticated through the real login flow of the application.
    /// </summary>
    /// <param name="factory">The Web application factory.</param>
    /// <param name="username">The username to authenticate with.</param>
    /// <param name="password">The password to authenticate with.</param>
    /// <returns>An HTTP client authenticated with the application's authentication cookie, together with an antiforgery token.</returns>
    public static async Task<AuthenticatedWebClient> CreateAuthenticatedClientAsync(WebApplicationFactory<Program> factory, string? username = "testuser", string? password = "TestPass123!")
    {
        // the application marks its cookies as secure, so the test client must communicate over HTTPS for the cookies to be sent back
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        string antiforgeryToken = await GetAntiforgeryTokenAsync(client);
        await LoginAsync(client, username!, password!, antiforgeryToken);
        // the antiforgery token is bound to the authenticated user, because the antiforgery middleware runs after
        // authentication; the token captured from the anonymous login page is rejected with a 400 for authenticated
        // mutating requests, so a fresh token must be captured after the login
        string authenticatedAntiforgeryToken = await GetAntiforgeryTokenAsync(client, AUTHENTICATED_TOKEN_PAGE_PATH);
        return new AuthenticatedWebClient(client, authenticatedAntiforgeryToken);
    }

    /// <summary>
    /// Performs the login flow of the application against the provided client.
    /// </summary>
    /// <param name="client">The HTTP client to log in through.</param>
    /// <param name="username">The username to authenticate with.</param>
    /// <param name="password">The password to authenticate with.</param>
    /// <param name="antiforgeryToken">The antiforgery token to send with the login request.</param>
    public static async Task LoginAsync(HttpClient client, string username, string password, string antiforgeryToken)
    {
        HttpRequestMessage loginRequest = new(HttpMethod.Post, LOGIN_ENDPOINT_PATH)
        {
            Content = JsonContent.Create(new LoginRequest(Username: username, Password: password))
        };
        // the antiforgery middleware matches the content type exactly, so the charset suffix must be omitted
        loginRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        loginRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        loginRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        HttpResponseMessage response = await client.SendAsync(loginRequest);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Retrieves the antiforgery token and cookie by requesting a page that renders an antiforgery token.
    /// </summary>
    /// <param name="client">The HTTP client to retrieve the token through.</param>
    /// <param name="pagePath">The page that renders the antiforgery token, defaults to the login page.</param>
    /// <returns>The antiforgery token value.</returns>
    public static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string? pagePath = null)
    {
        HttpResponseMessage loginPageResponse = await client.GetAsync(pagePath ?? LOGIN_PAGE_PATH);
        loginPageResponse.EnsureSuccessStatusCode();
        string loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();
        Match match = AntiforgeryTokenRegex().Match(loginPageHtml);
        if (!match.Success)
            throw new InvalidOperationException("Could not find the antiforgery token on the page.");
        return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    /// <summary>
    /// Deserializes a JSON response body into a <see cref="JsonDocument"/>.
    /// </summary>
    /// <param name="response">The HTTP response whose body is deserialized.</param>
    /// <returns>The parsed <see cref="JsonDocument"/>.</returns>
    public static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
