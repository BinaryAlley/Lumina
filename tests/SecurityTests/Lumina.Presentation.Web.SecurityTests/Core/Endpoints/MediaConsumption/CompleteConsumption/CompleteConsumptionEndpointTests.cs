#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.CompleteConsumption;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.MediaConsumption.CompleteConsumption;

/// <summary>
/// Contains security tests for the <see cref="CompleteConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly LuminaWebFactory _apiFactory;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly CompleteConsumptionRequestFixture _completeConsumptionRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CompleteConsumptionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CompleteConsumption_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage request = CreateCompleteRequest(s_recordId.ToString(), _completeConsumptionRequestFixture.Create(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetConsumptionEndpointCalls());
    }

    [Fact]
    public async Task CompleteConsumption_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateCompleteRequest(s_recordId.ToString(), _completeConsumptionRequestFixture.Create(), null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetConsumptionEndpointCalls());
    }

    [Fact]
    public async Task CompleteConsumption_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateCompleteRequest(s_recordId.ToString(), _completeConsumptionRequestFixture.Create(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetConsumptionEndpointCalls());
    }

    [Fact]
    public async Task CompleteConsumption_WhenCalledByAuthenticatedUserWithValidToken_ShouldCallApiOnceWithoutLeakingSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/complete";
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(status: ConsumptionStatus.Completed);
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, expectedRecord);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateCompleteRequest(s_recordId.ToString(), _completeConsumptionRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expectedEndpoint], GetConsumptionEndpointCalls());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task CompleteConsumption_WithInjectionPayloadInRecordId_ShouldUseTheEmptyGuidAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/consumption-records/{ZERO_GUID}/complete";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateCompleteRequest(Uri.EscapeDataString(payload), _completeConsumptionRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([sanitizedEndpoint], GetConsumptionEndpointCalls());
        Assert.DoesNotContain(payload, string.Join(",", GetConsumptionEndpointCalls()), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", GetConsumptionEndpointCalls()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CompleteConsumption_WhenApiFails_ShouldNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/consumption-records/{s_recordId}/complete";
        _apiFactory.ApiClientStub.RegisterPostException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateCompleteRequest(s_recordId.ToString(), _completeConsumptionRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the consumption endpoints that were called upstream, excluding the login request performed to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream consumption endpoints that were called.</returns>
    private List<string> GetConsumptionEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint != ApiRoutes.Authentication.LOGIN_ACCOUNT)
            .Select(postRequest => postRequest.Endpoint)];
    }

    /// <summary>
    /// Builds the complete consumption request for the specified record.
    /// </summary>
    /// <param name="recordId">The value of the record identifier route segment.</param>
    /// <param name="completeRequest">The completion payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured complete consumption request.</returns>
    private static HttpRequestMessage CreateCompleteRequest(string recordId, CompleteConsumptionRequest completeRequest, string? antiforgeryToken)
    {
        HttpRequestMessage completionRequest = new(HttpMethod.Post, $"/en-us/media-consumption/api-complete-consumption/{recordId}")
        {
            Content = JsonContent.Create(completeRequest)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        completionRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        completionRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            completionRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return completionRequest;
    }

    /// <summary>
    /// Releases the resources used by the test class and resets the remote API stub.
    /// </summary>
    public void Dispose()
    {
        _apiFactory.ApiClientStub.Reset();
        GC.SuppressFinalize(this);
    }
}
