#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using Lumina.Plugins.CoverArtArchive.Core;
using Lumina.Plugins.CoverArtArchive.Core.Api;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.DTO.Settings;
using Lumina.Plugins.CoverArtArchive.UnitTests.Common.TestHelpers;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Core.Api;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveHttpClient"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveHttpClientTests
{
    private const string ARTWORK_JSON = """
    {
        "images": [
            { "image": "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg", "front": true, "types": [ "Front" ] }
        ]
    }
    """;

    private static readonly Guid s_releaseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_releaseGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly CoverArtArchiveSettingsDtoFixture _coverArtArchiveSettingsDtoFixture = new();

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheReleaseHasArtwork_ShouldDeserializeIt()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient();

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Images);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg", Assert.Single(result.Images)!.Image);
        Assert.Equal($"/release/{s_releaseId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("coverartarchive.org", handler.Requests[0].RequestUri!.Host);
    }

    [Fact]
    public async Task GetReleaseGroupArtworkAsync_WhenTheReleaseGroupHasArtwork_ShouldDeserializeIt()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient();

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseGroupArtworkAsync(s_releaseGroupId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Images);
        Assert.Single(result.Images);
        Assert.Equal($"/release-group/{s_releaseGroupId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheReleaseIsNotFound_ShouldReturnNull()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheSameReleaseIsRequestedTwice_ShouldServeTheSecondRequestFromTheCache()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient();

        // Act
        CoverArtArchiveArtworkResponse? first = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        CoverArtArchiveArtworkResponse? second = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheReleaseIsNotFound_ShouldCacheTheAbsence()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        CoverArtArchiveArtworkResponse? first = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        CoverArtArchiveArtworkResponse? second = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.Null(first);
        Assert.Null(second);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTwoDifferentReleasesAreRequested_ShouldNotShareTheCache()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient();

        // Act
        await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        await client.GetReleaseArtworkAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheEndpointRedirectsToTheInternetArchive_ShouldFollowItAndDeserializeTheBody()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) =>
        {
            if (callIndex == 1)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri($"https://archive.org/download/mbid-{s_releaseId:D}/index.json");
                return redirect;
            }
            return CreateJsonResponse(ARTWORK_JSON);
        });

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal("archive.org", handler.Requests[1].RequestUri!.Host);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheRedirectLocationIsRelative_ShouldResolveItAgainstTheRequest()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) =>
        {
            if (callIndex == 1)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri($"/release/{s_releaseId:D}/index.json", UriKind.Relative);
                return redirect;
            }
            return CreateJsonResponse(ARTWORK_JSON);
        });

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal("coverartarchive.org", handler.Requests[1].RequestUri!.Host);
        Assert.Equal($"/release/{s_releaseId:D}/index.json", handler.Requests[1].RequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData("https://evil.example/index.json")] // an unrelated public host
    [InlineData("https://localhost/index.json")] // the loopback host name
    [InlineData("https://127.0.0.1/index.json")] // the IPv4 loopback address
    [InlineData("https://169.254.169.254/latest/meta-data/")] // the link local cloud metadata endpoint
    public async Task GetReleaseArtworkAsync_WhenTheRedirectPointsAtADisallowedHost_ShouldThrow(string location)
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri(location);
            return redirect;
        });

        // Act
        Task Act()
        {
            return client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheRedirectDowngradesToPlainHttp_ShouldThrow()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri("http://archive.org/index.json");
            return redirect;
        });

        // Act
        Task Act()
        {
            return client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheEndpointRedirectsTooManyTimes_ShouldThrow()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri("https://archive.org/loop/index.json");
            return redirect;
        });

        // Act
        Task Act()
        {
            return client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        // The initial request plus the maximum number of followed redirects are sent before the loop is detected.
        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheFirstAttemptReturns503_ShouldRetryAndSucceed()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) => callIndex == 1
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : CreateJsonResponse(ARTWORK_JSON));

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheFirstAttemptIsRateLimited_ShouldRetryAndSucceed()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) => callIndex == 1
            ? new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            : CreateJsonResponse(ARTWORK_JSON));

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheFirstAttemptThrowsATransportException_ShouldRetryAndSucceed()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) => callIndex == 1
            ? throw new HttpRequestException("Transient failure.")
            : CreateJsonResponse(ARTWORK_JSON));

        // Act
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenEveryAttemptFails_ShouldThrowAfterExhaustingTheRetries()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        Task Act()
        {
            return client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheFirstAttemptIsRateLimitedWithRetryAfter_ShouldHonorTheRequestedDelay()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient((request, callIndex) =>
        {
            if (callIndex != 1)
                return CreateJsonResponse(ARTWORK_JSON);
            HttpResponseMessage rateLimited = new(HttpStatusCode.TooManyRequests);
            rateLimited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
            return rateLimited;
        });

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        CoverArtArchiveArtworkResponse? result = await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
        // The requested delay of zero is honored instead of the default five hundred millisecond backoff of the first retry.
        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(400), $"The elapsed time {stopwatch.Elapsed} shows the Retry-After header was ignored.");
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheContactEmailIsConfigured_ShouldIncludeItInTheUserAgent()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient(contactEmail: "contact@example.com");

        // Act
        await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        string userAgent = handler.Requests[0].Headers.UserAgent.ToString();
        Assert.Contains("Lumina-Test/1.0", userAgent);
        Assert.Contains("contact@example.com", userAgent);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheContactEmailIsNotConfigured_ShouldNotIncludeItInTheUserAgent()
    {
        // Arrange
        (CoverArtArchiveHttpClient client, StubHttpMessageHandler handler) = CreateClient();

        // Act
        await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        string userAgent = handler.Requests[0].Headers.UserAgent.ToString();
        Assert.Equal("Lumina-Test/1.0", userAgent);
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheUserAgentIsAlreadySet_ShouldNotOverrideIt()
    {
        // Arrange
        StubHttpMessageHandler handler = new((request, callIndex) => CreateJsonResponse(ARTWORK_JSON));
        HttpClient httpClient = new(handler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Custom/9.9");
        CoverArtArchiveSettingsProvider settingsProvider = CreateSettingsProvider(contactEmail: null);
        CoverArtArchiveHttpClient client = new(httpClient, settingsProvider, new CoverArtArchiveRequestThrottle(), new CoverArtArchiveResponseCache());

        // Act
        await client.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        Assert.Equal("Custom/9.9", handler.Requests[0].Headers.UserAgent.ToString());
    }

    [Fact]
    public async Task GetReleaseArtworkAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldCancelTheOperation()
    {
        // Arrange
        using (CancellationTokenSource cancellationTokenSource = new())
        {
            cancellationTokenSource.Cancel();
            (CoverArtArchiveHttpClient client, _) = CreateClient();

            // Act
            Task Act()
            {
                return client.GetReleaseArtworkAsync(s_releaseId, cancellationTokenSource.Token);
            }

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
        }
    }

    [Fact]
    public void GetRetryDelay_WhenTheResponseIsNull_ShouldReturnTheExponentialBackoff()
    {
        // Act
        TimeSpan result = CoverArtArchiveHttpClient.GetRetryDelay(1, null);

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(500), result);
    }

    [Fact]
    public void GetRetryDelay_WhenTheResponseHasNoRetryAfter_ShouldReturnTheExponentialBackoff()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.InternalServerError);

        // Act
        TimeSpan result = CoverArtArchiveHttpClient.GetRetryDelay(2, response);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(1), result);
    }

    [Fact]
    public void GetRetryDelay_WhenTheResponseHasARelativeRetryAfter_ShouldReturnTheRequestedDelay()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5));

        // Act
        TimeSpan result = CoverArtArchiveHttpClient.GetRetryDelay(1, response);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(5), result);
    }

    [Fact]
    public void GetRetryDelay_WhenTheResponseHasARetryAfterOfZero_ShouldReturnZero()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);

        // Act
        TimeSpan result = CoverArtArchiveHttpClient.GetRetryDelay(1, response);

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void GetRetryDelay_WhenTheResponseHasAnAbsoluteRetryAfterInTheFuture_ShouldReturnTheRemainingTime()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(5));

        // Act
        TimeSpan result = CoverArtArchiveHttpClient.GetRetryDelay(1, response);

        // Assert
        Assert.True(result > TimeSpan.FromSeconds(4) && result <= TimeSpan.FromSeconds(5), $"The delay {result} was not the remaining time of the absolute Retry-After.");
    }

    [Fact]
    public void GetRetryDelay_WhenTheResponseHasAnAbsoluteRetryAfterInThePast_ShouldReturnTheExponentialBackoff()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(-5));

        // Act
        TimeSpan result = CoverArtArchiveHttpClient.GetRetryDelay(1, response);

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(500), result);
    }

    [Fact]
    public void GetRetryDelay_WhenTheRequestedDelayExceedsTheMaximum_ShouldCapIt()
    {
        // Arrange
        HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromDays(10));

        // Act
        TimeSpan result = CoverArtArchiveHttpClient.GetRetryDelay(1, response);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(60), result);
    }

    /// <summary>
    /// Creates a <see cref="CoverArtArchiveHttpClient"/> whose requests are answered by a stub handler, returning the artwork JSON by default.
    /// </summary>
    /// <param name="responseFactory">Optional. The factory that produces the response of a request.</param>
    /// <param name="contactEmail">Optional. The contact email configured for the client.</param>
    /// <returns>The created client and its stub handler.</returns>
    private (CoverArtArchiveHttpClient Client, StubHttpMessageHandler Handler) CreateClient(
        Func<HttpRequestMessage, int, HttpResponseMessage>? responseFactory = null,
        string? contactEmail = null)
    {
        StubHttpMessageHandler handler = new(responseFactory ?? ((request, callIndex) => CreateJsonResponse(ARTWORK_JSON)));
        CoverArtArchiveSettingsProvider settingsProvider = CreateSettingsProvider(contactEmail);
        CoverArtArchiveHttpClient client = new(new HttpClient(handler), settingsProvider, new CoverArtArchiveRequestThrottle(), new CoverArtArchiveResponseCache());
        return (client, handler);
    }

    /// <summary>
    /// Creates a settings provider configured for the tests, with no throttling.
    /// </summary>
    /// <param name="contactEmail">Optional. The contact email configured for the client.</param>
    /// <returns>The created settings provider.</returns>
    private CoverArtArchiveSettingsProvider CreateSettingsProvider(string? contactEmail)
    {
        return new CoverArtArchiveSettingsProvider(null, CoverArtArchivePlugin.s_pluginId, _coverArtArchiveSettingsDtoFixture.Create(
            userAgent: "Lumina-Test/1.0",
            contactEmail: contactEmail,
            includeContactEmail: contactEmail is not null,
            minimumRequestInterval: TimeSpan.Zero));
    }

    /// <summary>
    /// Creates an OK response with the provided JSON body.
    /// </summary>
    /// <param name="json">The JSON body of the response.</param>
    /// <returns>The created response.</returns>
    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
