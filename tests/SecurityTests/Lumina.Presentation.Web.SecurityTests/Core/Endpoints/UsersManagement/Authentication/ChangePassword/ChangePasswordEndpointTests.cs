#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.ChangePassword;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// Contains security tests for the <c>/{culture}/auth/api-change-password</c> route served by the <see cref="ChangePasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ChangePasswordRequestFixture _changePasswordRequestFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ChangePasswordEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ChangePassword_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        HttpRequestMessage changePasswordRequest = new(HttpMethod.Post, "/en-us/auth/api-change-password")
        {
            Content = JsonContent.Create(request)
        };
        changePasswordRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        changePasswordRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        changePasswordRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(changePasswordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/change-password");
    }

    [Fact]
    public async Task ChangePassword_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        HttpRequestMessage changePasswordRequest = new(HttpMethod.Post, "/en-us/auth/api-change-password")
        {
            Content = JsonContent.Create(request)
        };
        changePasswordRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        changePasswordRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(changePasswordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/change-password");
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("admin'--")] // comment injection
    [InlineData("' UNION SELECT * FROM Users--")] // union injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    [InlineData("{{7*7}}")] // template injection
    public async Task ChangePassword_WithInjectionAttemptInNewPassword_ShouldRemainSecure(string maliciousNewPassword)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/change-password", new ApiException(_problemDetailsDtoFixture.Create(title: "General.Failure", detail: "OperationFailed"), HttpStatusCode.Forbidden, "auth/change-password"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ChangePasswordRequest request = _changePasswordRequestFixture.Create(currentPassword: "OldPass123!", newPassword: maliciousNewPassword, newPasswordConfirm: maliciousNewPassword);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateChangePasswordRequest(request, webClient.AntiforgeryToken));
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
    public async Task ChangePassword_WhenApiReturnsError_ShouldReturnCleanError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/change-password", new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Failure", detail: "OperationFailed"),
            HttpStatusCode.Conflict));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateChangePasswordRequest(request, webClient.AntiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
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
