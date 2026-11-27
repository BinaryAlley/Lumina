#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Enums.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Scheduler.RemoveScheduledJob;

/// <summary>
/// Contains security tests for the <see cref="RemoveScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RemoveScheduledJobEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RemoveScheduledJob_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        string url = $"/en-us/admin/api-scheduled-jobs/{Guid.NewGuid()}";
        HttpRequestMessage request = new(HttpMethod.Delete, url)
        {
            Content = CreateJsonContent()
        };

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.DeleteEndpointsCalled, endpoint => endpoint.StartsWith("scheduled-jobs", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RemoveScheduledJob_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        string url = $"/en-us/admin/api-scheduled-jobs/{Guid.NewGuid()}";
        HttpRequestMessage request = new(HttpMethod.Delete, url)
        {
            Content = CreateJsonContent()
        };
        request.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.DeleteEndpointsCalled, endpoint => endpoint.StartsWith("scheduled-jobs", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RemoveScheduledJob_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        string url = $"/en-us/admin/api-scheduled-jobs/{Guid.NewGuid()}";
        HttpRequestMessage request = new(HttpMethod.Delete, url)
        {
            Content = CreateJsonContent()
        };
        request.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.DeleteEndpointsCalled, endpoint => endpoint.StartsWith("scheduled-jobs", StringComparison.Ordinal));
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