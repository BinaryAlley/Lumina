#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;

/// <summary>
/// Contains integration tests for the <see cref="ClearConsumptionHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private static readonly string s_userId = Guid.NewGuid().ToString();

    private readonly LuminaWebFactory _apiFactory;
    private readonly ClearConsumptionHistoryRequestFixture _clearConsumptionHistoryRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ClearConsumptionHistoryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ClearConsumptionHistory_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldDeleteCurrentUserHistoryAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ClearConsumptionHistoryRequest request = _clearConsumptionHistoryRequestFixture.Create(includeUserId: false);
        string expectedUpstreamPath = BuildExpectedUpstreamPath(request);
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedUpstreamPath);
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
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        Assert.Equal([expectedUpstreamPath], _apiFactory.ApiClientStub.DeleteEndpointsCalled);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task ClearConsumptionHistory_WhenUserIdIsProvided_ShouldDeleteSelectedUserHistory()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ClearConsumptionHistoryRequest request = _clearConsumptionHistoryRequestFixture.Create(userId: s_userId);
        string expectedUpstreamPath = BuildExpectedUpstreamPath(request);
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedUpstreamPath);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([$"users/{s_userId}/consumption-history"], _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task ClearConsumptionHistory_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ClearConsumptionHistoryRequest request = _clearConsumptionHistoryRequestFixture.Create(userId: s_userId);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
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
    /// Builds the path of the Web route that clears the consumption history described by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request describing the history to clear.</param>
    /// <returns>The path of the Web route, including the optional query string.</returns>
    private static string BuildRequestPath(ClearConsumptionHistoryRequest request)
    {
        return string.IsNullOrWhiteSpace(request.UserId)
            ? "/en-us/library/statistics/api-clear-consumption-history"
            : $"/en-us/library/statistics/api-clear-consumption-history?userId={Uri.EscapeDataString(request.UserId)}";
    }

    /// <summary>
    /// Builds the upstream API path that is expected for <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request describing the history to clear.</param>
    /// <returns>The upstream API path, including the optional escaped user identifier.</returns>
    private static string BuildExpectedUpstreamPath(ClearConsumptionHistoryRequest request)
    {
        return string.IsNullOrWhiteSpace(request.UserId)
            ? "users/me/consumption-history"
            : $"users/{Uri.EscapeDataString(request.UserId)}/consumption-history";
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
