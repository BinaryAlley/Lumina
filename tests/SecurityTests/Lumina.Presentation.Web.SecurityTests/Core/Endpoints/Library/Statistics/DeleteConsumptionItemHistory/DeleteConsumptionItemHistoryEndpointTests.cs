#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// Contains security tests for the <see cref="DeleteConsumptionItemHistoryEndpoint"/> class.
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
    public async Task DeleteConsumptionItemHistory_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        DeleteConsumptionItemHistoryRequest request = CreateRequest(s_userId);
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
    public async Task DeleteConsumptionItemHistory_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        DeleteConsumptionItemHistoryRequest request = CreateRequest(s_userId);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteConsumptionItemHistory_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        DeleteConsumptionItemHistoryRequest request = CreateRequest(s_userId);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(BuildRequestPath(request));
        deleteRequest.Headers.Add("RequestVerificationToken", "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Theory]
    [InlineData("Movie%2F..%2Fadmin%3Fx%3D1")] // path traversal attempt in the media kind
    [InlineData("NotAMediaKind")] // undefined enum member
    public async Task DeleteConsumptionItemHistory_WithCraftedMediaKind_ShouldRejectRequestAndNotCallApi(string craftedMediaKind)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        string path = $"/en-us/library/statistics/api-delete-consumption-item-history?userId={Uri.EscapeDataString(s_userId)}&mediaKind={craftedMediaKind}&mediaItemId={s_mediaItemId}&libraryId={s_libraryId}";
        HttpRequestMessage deleteRequest = CreateDeleteRequest(path);
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteConsumptionItemHistory_WithCraftedUserId_ShouldEscapeTheValueAndKeepTheDocumentedUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string CRAFTED_USER_ID = "user/../admin?x=1&y=2";
        const string EXPECTED_UPSTREAM_PATH = "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/consumption-history/media-items/Movie/11111111-1111-1111-1111-111111111111/22222222-2222-2222-2222-222222222222";
        DeleteConsumptionItemHistoryRequest request = CreateRequest(CRAFTED_USER_ID);
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
    /// Creates the request used by the tests for the specified user.
    /// </summary>
    /// <param name="userId">The Id of the user whose item history is deleted.</param>
    /// <returns>The configured request.</returns>
    private DeleteConsumptionItemHistoryRequest CreateRequest(string userId)
    {
        return _deleteConsumptionItemHistoryRequestFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: s_mediaItemId,
            libraryId: s_libraryId);
    }

    /// <summary>
    /// Builds the path of the Web route that deletes the item history described by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request describing the item history to delete.</param>
    /// <returns>The path of the Web route, including the escaped query string.</returns>
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
