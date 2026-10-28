#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.ClearResumePoints;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Statistics.ClearResumePoints;

/// <summary>
/// Contains security tests for the <see cref="ClearResumePointsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private static readonly string s_userId = Guid.NewGuid().ToString();

    private readonly LuminaWebFactory _apiFactory;
    private readonly ClearResumePointsRequestFixture _clearResumePointsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ClearResumePointsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ClearResumePoints_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(userId: s_userId);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));
        deleteRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task ClearResumePoints_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(userId: s_userId);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task ClearResumePoints_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(userId: s_userId);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));
        deleteRequest.Headers.Add("RequestVerificationToken", "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task ClearResumePoints_WithCraftedUserAndLibraryIds_ShouldEscapeTheValuesAndKeepTheDocumentedUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string CRAFTED_USER_ID = "user/../admin?x=1&y=2";
        const string CRAFTED_LIBRARY_ID = "lib?x=1&y=2";
        const string EXPECTED_UPSTREAM_PATH = "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/media-resume-points?libraryId=lib%3Fx%3D1%26y%3D2";
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(userId: CRAFTED_USER_ID, libraryId: CRAFTED_LIBRARY_ID);
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(EXPECTED_UPSTREAM_PATH);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal([EXPECTED_UPSTREAM_PATH], _apiFactory.ApiClientStub.DeleteEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Releases the resources used by the test class and resets the remote API stub.
    /// </summary>
    public void Dispose()
    {
        _apiFactory.ApiClientStub.Reset();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Builds the path of the Web route that clears the resume points described by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request describing the resume points to clear.</param>
    /// <returns>The path of the Web route, including the escaped query string.</returns>
    private static string BuildRequestPath(ClearResumePointsRequest request)
    {
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(request.UserId))
            parameters.Add($"userId={Uri.EscapeDataString(request.UserId)}");
        if (!string.IsNullOrWhiteSpace(request.LibraryId))
            parameters.Add($"libraryId={Uri.EscapeDataString(request.LibraryId)}");
        return parameters.Count == 0
            ? "/en-us/library/statistics/api-clear-resume-points"
            : $"/en-us/library/statistics/api-clear-resume-points?{string.Join("&", parameters)}";
    }

    /// <summary>
    /// Builds a DELETE request for <paramref name="path"/> that carries the JSON content type required by the antiforgery middleware.
    /// </summary>
    /// <param name="path">The path of the Web route to call.</param>
    /// <returns>The configured DELETE request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(string path)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, path)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        deleteRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return deleteRequest;
    }
}
