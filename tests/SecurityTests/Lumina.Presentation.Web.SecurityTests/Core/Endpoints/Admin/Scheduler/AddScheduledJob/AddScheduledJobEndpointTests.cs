#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Scheduler.AddScheduledJob;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/api-scheduled-jobs/add</c> route served by the <see cref="AddScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddScheduledJobEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AddScheduledJob_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        string url = "/en-us/admin/api-scheduled-jobs/add";
        HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = CreateJsonContent()
        };

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.StartsWith("scheduled-jobs", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AddScheduledJob_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        string url = "/en-us/admin/api-scheduled-jobs/add";
        HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = CreateJsonContent()
        };
        request.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.StartsWith("scheduled-jobs", StringComparison.Ordinal));
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