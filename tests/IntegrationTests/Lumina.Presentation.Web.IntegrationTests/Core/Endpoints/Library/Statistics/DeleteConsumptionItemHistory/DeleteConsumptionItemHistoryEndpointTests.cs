#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// Contains integration tests for the <see cref="DeleteConsumptionItemHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private static readonly string s_userId = Guid.NewGuid().ToString();
    private static readonly Guid s_mediaItemId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_libraryId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly LuminaWebFactory _apiFactory;
    private readonly DeleteConsumptionItemHistoryRequestFixture _deleteConsumptionItemHistoryRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeleteConsumptionItemHistoryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DeleteConsumptionItemHistory_WhenCalledForCurrentUser_ShouldDeleteCurrentUserItemHistoryAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            includeUserId: false,
            mediaKind: MediaKind.Movie,
            mediaItemId: s_mediaItemId,
            libraryId: s_libraryId);
        string expectedUpstreamPath = $"users/me/consumption-history/media-items/Movie/{s_mediaItemId}/{s_libraryId}";
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
    public async Task DeleteConsumptionItemHistory_WhenUserIsProvided_ShouldDeleteSelectedUserItemHistory()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            userId: s_userId,
            mediaKind: MediaKind.Book,
            mediaItemId: s_mediaItemId,
            libraryId: s_libraryId);
        string expectedUpstreamPath = $"users/{s_userId}/consumption-history/media-items/Book/{s_mediaItemId}/{s_libraryId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedUpstreamPath);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expectedUpstreamPath], _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteConsumptionItemHistory_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            userId: s_userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: s_mediaItemId,
            libraryId: s_libraryId);
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
    /// Builds the path of the Web route that deletes the item history described by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request describing the item history to delete.</param>
    /// <returns>The path of the Web route, including the query string.</returns>
    private static string BuildRequestPath(DeleteConsumptionItemHistoryRequest request)
    {
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(request.UserId))
            parameters.Add($"userId={Uri.EscapeDataString(request.UserId)}");
        parameters.Add($"mediaKind={Uri.EscapeDataString(request.MediaKind.ToString())}");
        parameters.Add($"mediaItemId={request.MediaItemId}");
        parameters.Add($"libraryId={request.LibraryId}");
        return $"/en-us/library/statistics/api-delete-consumption-item-history?{string.Join("&", parameters)}";
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
