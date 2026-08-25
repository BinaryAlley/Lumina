#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Contains security tests for the <c>/{culture}/auth/api-recover-password</c> route served by the <see cref="RecoverPasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly RecoverPasswordRequestFixture _recoverPasswordRequestFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RecoverPasswordEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("admin'--")] // comment injection
    [InlineData("' UNION SELECT * FROM Users--")] // union injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    [InlineData("{{7*7}}")] // template injection
    public async Task RecoverPassword_WithInjectionAttemptInUsername_ShouldRemainSecure(string maliciousUsername)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/recover-password", new ApiException(_problemDetailsDtoFixture.Create(title: "General.Failure", detail: "RecoveryFailed"), HttpStatusCode.Forbidden, "auth/recover-password"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create(username: maliciousUsername, totpCode: "123456");

        // Act
        HttpResponseMessage response = await client.SendAsync(CreateRecoverPasswordRequest(request, antiforgeryToken));
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
    public async Task RecoverPassword_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create(username: "testuser", totpCode: "123456");
        HttpRequestMessage recoverPasswordRequest = new(HttpMethod.Post, "/en-us/auth/api-recover-password")
        {
            Content = JsonContent.Create(request)
        };
        recoverPasswordRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(recoverPasswordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "auth/recover-password");
    }

    [Fact]
    public async Task RecoverPassword_WhenApiReturnsError_ShouldReturnCleanError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/recover-password", new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Failure", detail: "RecoveryFailed"),
            HttpStatusCode.Conflict));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create(username: "testuser", totpCode: "123456");

        // Act
        HttpResponseMessage response = await client.SendAsync(CreateRecoverPasswordRequest(request, antiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
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
