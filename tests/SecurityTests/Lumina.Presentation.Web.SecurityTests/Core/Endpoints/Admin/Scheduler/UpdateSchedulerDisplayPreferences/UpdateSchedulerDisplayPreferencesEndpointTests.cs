#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Scheduler.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Contains security tests for the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdateSchedulerDisplayPreferencesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateSchedulerDisplayPreferences_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        string url = "/en-us/admin/api-scheduled-jobs/display-preferences";
        HttpRequestMessage request = new(HttpMethod.Put, url)
        {
            Content = CreateJsonContent()
        };

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task UpdateSchedulerDisplayPreferences_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        string url = "/en-us/admin/api-scheduled-jobs/display-preferences";
        HttpRequestMessage request = new(HttpMethod.Put, url)
        {
            Content = CreateJsonContent()
        };
        request.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task UpdateSchedulerDisplayPreferences_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        string url = "/en-us/admin/api-scheduled-jobs/display-preferences";
        HttpRequestMessage request = new(HttpMethod.Put, url)
        {
            Content = CreateJsonContent()
        };
        request.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Creates the empty JSON body of the request; the content type is set without the charset suffix, because the antiforgery middleware matches the content type exactly.
    /// </summary>
    /// <returns>The created content.</returns>
    private static StringContent CreateJsonContent()
    {
        StringContent content = new("{}", Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }
}
