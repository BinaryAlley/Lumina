#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using Lumina.Plugins.MusicBrainz.Core;
using Lumina.Plugins.MusicBrainz.Core.Api;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.UnitTests.Common.TestHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Core.Api;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzHttpClient"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzHttpClientTests
{
    private const string RECORDING_JSON = """{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"Death on Two Legs","video":null}""";

    private readonly MusicBrainzSettingsDtoFixture _musicBrainzSettingsDtoFixture = new();

    [Fact]
    public async Task GetRecordingAsync_WhenTheSameRecordingIsRequestedTwice_ShouldServeTheSecondRequestFromTheCache()
    {
        // Arrange
        Guid recordingId = Guid.NewGuid();
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero);

        // Act
        MusicBrainzRecordingResponse? first = await client.GetRecordingAsync(recordingId, CancellationToken.None);
        MusicBrainzRecordingResponse? second = await client.GetRecordingAsync(recordingId, CancellationToken.None);

        // Assert
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheBaseUrlIsNotTheRealService_ShouldNotThrottleTheRequests()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost:5000/", TimeSpan.FromSeconds(1));

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);
        await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);
        stopwatch.Stop();

        // Assert
        // The configured minimum request interval is one second, but a base URL that is not the real MusicBrainz service is not throttled.
        Assert.Equal(2, handler.CallCount);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(500));
    }

    [Theory]
    [InlineData("https://musicbrainz.org/ws/2/")] // the real service
    [InlineData("https://mirror.musicbrainz.org/ws/2/")] // a subdomain of the real service
    public async Task GetRecordingAsync_WhenTheBaseUrlIsTheRealService_ShouldThrottleTheRequests(string baseUrl)
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, HttpClient _) = CreateClient(baseUrl, TimeSpan.FromMilliseconds(100));

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);
        await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);
        stopwatch.Stop();

        // Assert
        Assert.Equal(2, handler.CallCount);
        Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(50), $"The elapsed time {stopwatch.Elapsed} was shorter than the expected throttle interval.");
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheBaseUrlIsNotAnAbsoluteUri_ShouldThrow()
    {
        // Arrange
        (MusicBrainzHttpClient client, _, _) = CreateClient("not-a-url", TimeSpan.Zero);

        // Act
        Task Act()
        {
            return client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<UriFormatException>(Act);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheRequestIsNotAuthorized_ShouldReturnNull()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        MusicBrainzRecordingResponse? result = await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldCancelTheOperation()
    {
        // Arrange
        using (CancellationTokenSource cancellationTokenSource = new())
        {
            cancellationTokenSource.Cancel();
            (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero);

            // Act
            Task Act()
            {
                return client.GetRecordingAsync(Guid.NewGuid(), cancellationTokenSource.Token);
            }

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
        }
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheFirstAttemptReturns503_ShouldRetryAndSucceed()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => callIndex == 1
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : Json(RECORDING_JSON));

        // Act
        MusicBrainzRecordingResponse? result = await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheFirstAttemptIsRateLimited_ShouldRetryAndSucceed()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => callIndex == 1
            ? new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            : Json(RECORDING_JSON));

        // Act
        MusicBrainzRecordingResponse? result = await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheFirstAttemptThrowsATransportException_ShouldRetryAndSucceed()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => callIndex == 1
            ? throw new HttpRequestException("Transient failure.")
            : Json(RECORDING_JSON));

        // Act
        MusicBrainzRecordingResponse? result = await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenEveryAttemptFails_ShouldThrowAfterExhaustingTheRetries()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        Task Act()
        {
            return client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheContactEmailIsConfigured_ShouldIncludeItInTheUserAgent()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, contactEmail: "contact@example.com");

        // Act
        await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        string userAgent = handler.Requests[0].Headers.UserAgent.ToString();
        Assert.Contains("Lumina-Test/1.0", userAgent);
        Assert.Contains("contact@example.com", userAgent);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheContactEmailIsNotConfigured_ShouldNotIncludeItInTheUserAgent()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero);

        // Act
        await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        string userAgent = handler.Requests[0].Headers.UserAgent.ToString();
        Assert.Equal("Lumina-Test/1.0", userAgent);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheUserAgentIsAlreadySet_ShouldNotOverrideIt()
    {
        // Arrange
        StubHttpMessageHandler handler = new((request, callIndex) => Json(RECORDING_JSON));
        HttpClient httpClient = new(handler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Custom/9.9");
        MusicBrainzHttpClient client = CreateMusicBrainzHttpClient(httpClient, "http://localhost/", TimeSpan.Zero, null);

        // Act
        await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal("Custom/9.9", handler.Requests[0].Headers.UserAgent.ToString());
    }

    [Fact]
    public async Task GetArtistAsync_WhenTheArtistExists_ShouldDeserializeIt()
    {
        // Arrange
        const string ARTIST_JSON = """{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","name":"Queen","type":"Group"}""";
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(ARTIST_JSON));

        // Act
        MusicBrainzArtistResponse? result = await client.GetArtistAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Queen", result.Name);
        Assert.Contains("inc=aliases+tags+genres+url-rels+artist-rels+ratings", handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task SearchArtistsAsync_WhenTheSearchReturnsArtists_ShouldReturnThem()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"artists":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","name":"Queen"}]}""";
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(SEARCH_JSON));

        // Act
        IReadOnlyList<MusicBrainzArtistResponse> result = await client.SearchArtistsAsync("Queen", 10, CancellationToken.None);

        // Assert
        Assert.Equal("Queen", Assert.Single(result).Name);
        Assert.Contains("limit=10", handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task SearchArtistsAsync_WhenTheResponseBodyIsNull_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json("null"));

        // Act
        IReadOnlyList<MusicBrainzArtistResponse> result = await client.SearchArtistsAsync("Queen", 10, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0, "limit=1")] // a limit below the minimum is clamped up
    [InlineData(200, "limit=100")] // a limit above the maximum is clamped down
    [InlineData(50, "limit=50")] // a limit within the range is kept
    public async Task SearchArtistsAsync_WhenTheLimitIsOutOfRange_ShouldClampIt(int requestedLimit, string expectedQuery)
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json("""{"artists":[]}"""));

        // Act
        await client.SearchArtistsAsync("Queen", requestedLimit, CancellationToken.None);

        // Assert
        Assert.Contains(expectedQuery, handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task GetReleaseGroupAsync_WhenTheReleaseGroupExists_ShouldDeserializeIt()
    {
        // Arrange
        const string RELEASE_GROUP_JSON = """{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"A Night at the Opera"}""";
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(RELEASE_GROUP_JSON));

        // Act
        MusicBrainzReleaseGroupResponse? result = await client.GetReleaseGroupAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("A Night at the Opera", result.Title);
    }

    [Fact]
    public async Task SearchReleaseGroupsAsync_WhenTheSearchReturnsReleaseGroups_ShouldReturnThem()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"release-groups":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"A Night at the Opera"}]}""";
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(SEARCH_JSON));

        // Act
        IReadOnlyList<MusicBrainzReleaseGroupResponse> result = await client.SearchReleaseGroupsAsync("Opera", 10, CancellationToken.None);

        // Assert
        Assert.Equal("A Night at the Opera", Assert.Single(result).Title);
    }

    [Fact]
    public async Task SearchReleaseGroupsAsync_WhenTheResponseBodyIsNull_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json("null"));

        // Act
        IReadOnlyList<MusicBrainzReleaseGroupResponse> result = await client.SearchReleaseGroupsAsync("Opera", 10, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReleaseAsync_WhenTheReleaseExists_ShouldDeserializeIt()
    {
        // Arrange
        const string RELEASE_JSON = """{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"A Night at the Opera"}""";
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(RELEASE_JSON));

        // Act
        MusicBrainzReleaseResponse? result = await client.GetReleaseAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("A Night at the Opera", result.Title);
    }

    [Fact]
    public async Task BrowseReleasesByReleaseGroupAsync_WhenReleasesExist_ShouldReturnThem()
    {
        // Arrange
        const string BROWSE_JSON = """{"release-count":1,"releases":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"A Night at the Opera"}]}""";
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(BROWSE_JSON));

        // Act
        IReadOnlyList<MusicBrainzReleaseResponse> result = await client.BrowseReleasesByReleaseGroupAsync(Guid.NewGuid(), 25, CancellationToken.None);

        // Assert
        Assert.Equal("A Night at the Opera", Assert.Single(result).Title);
        Assert.Contains("inc=artist-credits+labels+media", handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task BrowseReleasesByReleaseGroupAsync_WhenTheResponseBodyIsNull_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json("null"));

        // Act
        IReadOnlyList<MusicBrainzReleaseResponse> result = await client.BrowseReleasesByReleaseGroupAsync(Guid.NewGuid(), 25, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchRecordingsAsync_WhenTheSearchReturnsRecordings_ShouldReturnThem()
    {
        // Arrange
        const string SEARCH_JSON = """{"count":1,"recordings":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"Death on Two Legs"}]}""";
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(SEARCH_JSON));

        // Act
        IReadOnlyList<MusicBrainzRecordingResponse> result = await client.SearchRecordingsAsync("Death", 10, CancellationToken.None);

        // Assert
        Assert.Equal("Death on Two Legs", Assert.Single(result).Title);
    }

    [Fact]
    public async Task SearchRecordingsAsync_WhenTheResponseBodyIsNull_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json("null"));

        // Act
        IReadOnlyList<MusicBrainzRecordingResponse> result = await client.SearchRecordingsAsync("Death", 10, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecordingsByIsrcAsync_WhenRecordingsExist_ShouldReturnThem()
    {
        // Arrange
        const string ISRC_JSON = """{"recordings":[{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"Death on Two Legs"}]}""";
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(ISRC_JSON));

        // Act
        IReadOnlyList<MusicBrainzRecordingResponse> result = await client.GetRecordingsByIsrcAsync("US1A2B3C4D5E", CancellationToken.None);

        // Assert
        Assert.Equal("Death on Two Legs", Assert.Single(result).Title);
    }

    [Fact]
    public async Task GetRecordingsByIsrcAsync_WhenTheResponseBodyIsNull_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json("null"));

        // Act
        IReadOnlyList<MusicBrainzRecordingResponse> result = await client.GetRecordingsByIsrcAsync("US1A2B3C4D5E", CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWorkAsync_WhenTheWorkExists_ShouldDeserializeIt()
    {
        // Arrange
        const string WORK_JSON = """{"id":"cb6c5931-436c-38c5-9d24-044f37d1fe35","title":"Death on Two Legs","type":"Song"}""";
        (MusicBrainzHttpClient client, _, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(WORK_JSON));

        // Act
        MusicBrainzWorkResponse? result = await client.GetWorkAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Death on Two Legs", result.Title);
    }

    [Fact]
    public async Task GetReleaseWithRecordingsAsync_WhenTheReleaseExists_ShouldDeserializeItsTracks()
    {
        // Arrange
        const string RELEASE_JSON = """
        {
            "id": "cb6c5931-436c-38c5-9d24-044f37d1fe35",
            "title": "A Night at the Opera",
            "media": [ { "position": 1, "track-count": 1, "tracks": [ { "position": 1, "title": "Death on Two Legs", "recording": { "id": "cb6c5931-436c-38c5-9d24-044f37d1fe35", "title": "Death on Two Legs" } } ] } ]
        }
        """;
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) => Json(RELEASE_JSON));

        // Act
        MusicBrainzReleaseResponse? result = await client.GetReleaseWithRecordingsAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("inc=recordings", handler.Requests[0].RequestUri!.Query);
        Assert.Single(Assert.Single(result.Media).Tracks);
    }

    [Fact]
    public async Task GetRecordingAsync_WhenTheFirstAttemptIsRateLimitedWithRetryAfter_ShouldHonorTheRequestedDelay()
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://localhost/", TimeSpan.Zero, (request, callIndex) =>
        {
            if (callIndex != 1)
                return Json(RECORDING_JSON);
            HttpResponseMessage rateLimited = new(HttpStatusCode.TooManyRequests);
            rateLimited.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
            return rateLimited;
        });

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        MusicBrainzRecordingResponse? result = await client.GetRecordingAsync(Guid.NewGuid(), CancellationToken.None);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, handler.CallCount);
        // The requested delay of zero is honored instead of the default five hundred millisecond backoff of the first retry.
        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(400), $"The elapsed time {stopwatch.Elapsed} shows the Retry-After header was ignored.");
    }

    [Theory]
    [InlineData("A & B")] // ampersand query separator
    [InlineData("Title #fragment")] // fragment separator
    [InlineData("A?B")] // query separator
    [InlineData("../../secret")] // path traversal
    [InlineData("A\" OR \"B")] // quote escape
    public async Task SearchArtistsAsync_WhenTheQueryContainsReservedCharacters_ShouldKeepThemInsideTheQueryString(string searchTerm)
    {
        // Arrange
        (MusicBrainzHttpClient client, StubHttpMessageHandler handler, _) = CreateClient("http://musicbrainz.example/ws/2/", TimeSpan.Zero, (request, callIndex) => Json("""{"artists":[]}"""));

        // Act
        await client.SearchArtistsAsync(searchTerm, 10, CancellationToken.None);

        // Assert
        Uri requestUri = handler.Requests[0].RequestUri!;
        Assert.Equal("musicbrainz.example", requestUri.Host);
        Assert.Equal("/ws/2/artist", requestUri.AbsolutePath);
        Assert.Equal(string.Empty, requestUri.Fragment);
    }

    /// <summary>
    /// Creates a <see cref="MusicBrainzHttpClient"/> whose requests are answered by a stub handler, returning the recording JSON by default.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MusicBrainz web service the client points to.</param>
    /// <param name="minimumRequestInterval">The minimum request interval configured for the client.</param>
    /// <param name="responseFactory">Optional. The factory that produces the response of a request.</param>
    /// <param name="contactEmail">Optional. The contact email configured for the client.</param>
    /// <returns>The created client, its stub handler and the underlying HTTP client.</returns>
    private (MusicBrainzHttpClient Client, StubHttpMessageHandler Handler, HttpClient HttpClient) CreateClient(
        string baseUrl,
        TimeSpan minimumRequestInterval,
        Func<HttpRequestMessage, int, HttpResponseMessage>? responseFactory = null,
        string? contactEmail = null)
    {
        StubHttpMessageHandler handler = new(responseFactory ?? ((request, callIndex) => Json(RECORDING_JSON)));
        HttpClient httpClient = new(handler);
        MusicBrainzHttpClient client = CreateMusicBrainzHttpClient(httpClient, baseUrl, minimumRequestInterval, contactEmail);
        return (client, handler, httpClient);
    }

    /// <summary>
    /// Creates a <see cref="MusicBrainzHttpClient"/> around the provided <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The underlying HTTP client.</param>
    /// <param name="baseUrl">The base URL of the MusicBrainz web service the client points to.</param>
    /// <param name="minimumRequestInterval">The minimum request interval configured for the client.</param>
    /// <param name="contactEmail">Optional. The contact email configured for the client.</param>
    /// <returns>The created client.</returns>
    private MusicBrainzHttpClient CreateMusicBrainzHttpClient(HttpClient httpClient, string baseUrl, TimeSpan minimumRequestInterval, string? contactEmail)
    {
        MusicBrainzSettingsProvider settingsProvider = new(null, MusicBrainzPlugin.s_pluginId, _musicBrainzSettingsDtoFixture.Create(
            baseUrl: baseUrl,
            userAgent: "Lumina-Test/1.0",
            contactEmail: contactEmail,
            includeContactEmail: contactEmail is not null,
            searchResultLimit: 10,
            releaseLookupLimit: 25,
            minimumRequestInterval: minimumRequestInterval));
        return new MusicBrainzHttpClient(httpClient, settingsProvider, new MusicBrainzRequestThrottle(), new MusicBrainzResponseCache());
    }

    /// <summary>
    /// Creates an OK response with the provided JSON body.
    /// </summary>
    /// <param name="json">The JSON body of the response.</param>
    /// <returns>The created response.</returns>
    private static HttpResponseMessage Json(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
