#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetTagConsumption;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Statistics.GetTagConsumption;

/// <summary>
/// Contains security tests for the <see cref="GetTagConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTagConsumptionEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/library/statistics/api-get-tag-consumption";

    private readonly LuminaWebFactory _apiFactory;
    private readonly GetTagConsumptionRequestFixture _getTagConsumptionRequestFixture = new();
    private readonly TagConsumptionDtoFixture _tagConsumptionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetTagConsumptionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetTagConsumption_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, WEB_ROUTE);
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetTagConsumption_WithCraftedUserAndLibraryIds_ShouldEscapeTheValuesAndKeepTheDocumentedUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string CRAFTED_USER_ID = "user/../admin?x=1&y=2";
        const string CRAFTED_LIBRARY_ID = "lib?x=1&y=2";
        const string EXPECTED_UPSTREAM_PATH = "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/consumption-statistics/tags?libraryId=lib%3Fx%3D1%26y%3D2";
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create(
            userId: CRAFTED_USER_ID,
            libraryId: CRAFTED_LIBRARY_ID,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        TagConsumptionDto[] expectedTags = [_tagConsumptionDtoFixture.Create()];
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_PATH, expectedTags);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildRequestPath(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        string[] dataEndpoints = [.. _apiFactory.ApiClientStub.GetEndpointsCalled.Where(endpoint => endpoint.StartsWith("users/", StringComparison.Ordinal))];
        Assert.Equal([EXPECTED_UPSTREAM_PATH], dataEndpoints);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
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
    /// Builds the path of the Web route for <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request whose filters are forwarded.</param>
    /// <returns>The path of the Web route, including the query string built from the request values.</returns>
    private static string BuildRequestPath(GetTagConsumptionRequest request)
    {
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(request.UserId))
            parameters.Add($"userId={Uri.EscapeDataString(request.UserId)}");
        if (!string.IsNullOrWhiteSpace(request.LibraryId))
            parameters.Add($"libraryId={Uri.EscapeDataString(request.LibraryId)}");
        return parameters.Count == 0
            ? WEB_ROUTE
            : $"{WEB_ROUTE}?{string.Join("&", parameters)}";
    }
}
