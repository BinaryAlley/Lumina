#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.Core.Api;

/// <summary>
/// HTTP client that calls the MusicBrainz web service and deserializes its JSON responses into typed response models.
/// </summary>
internal sealed class MusicBrainzHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly MusicBrainzSettingsProvider _settingsProvider;
    private readonly MusicBrainzRequestThrottle _requestThrottle;
    private readonly MusicBrainzResponseCache _responseCache;

    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    // Transient failures and rate limiting are retried with an exponential backoff, so a single hiccup does not mark an item as failed to enrich.
    private const int MAXIMUM_ATTEMPT_COUNT = 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicBrainzHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to send requests to the MusicBrainz web service.</param>
    /// <param name="settingsProvider">The provider of the settings that configure the MusicBrainz web service requests.</param>
    /// <param name="requestThrottle">The process-wide throttle that spaces out the requests sent to the MusicBrainz web service.</param>
    /// <param name="responseCache">The process-wide cache that avoids fetching the same entity more than once during a run.</param>
    public MusicBrainzHttpClient(HttpClient httpClient, MusicBrainzSettingsProvider settingsProvider, MusicBrainzRequestThrottle requestThrottle, MusicBrainzResponseCache responseCache)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        _requestThrottle = requestThrottle;
        _responseCache = responseCache;
    }

    /// <summary>
    /// Looks up the artist identified by <paramref name="musicBrainzArtistId"/>.
    /// </summary>
    /// <param name="musicBrainzArtistId">The MusicBrainz identifier of the artist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artist, or <see langword="null"/> when no artist was found.</returns>
    public Task<MusicBrainzArtistResponse?> GetArtistAsync(Guid musicBrainzArtistId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<MusicBrainzArtistResponse>(
            $"artist/{musicBrainzArtistId:D}?inc=aliases+tags+genres+url-rels+artist-rels+ratings&fmt=json", cancellationToken);
    }

    /// <summary>
    /// Searches MusicBrainz for artists matching the provided query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of matching artists.</returns>
    public async Task<IReadOnlyList<MusicBrainzArtistResponse>> SearchArtistsAsync(string query, int limit, CancellationToken cancellationToken)
    {
        MusicBrainzArtistSearchResponse? response = await GetJsonOrNullAsync<MusicBrainzArtistSearchResponse>(
            $"artist?query={Uri.EscapeDataString(query)}&limit={Math.Clamp(limit, 1, 100).ToString(CultureInfo.InvariantCulture)}&fmt=json",
            cancellationToken).ConfigureAwait(false);
        return response?.Artists ?? [];
    }

    /// <summary>
    /// Looks up the release group identified by <paramref name="musicBrainzReleaseGroupId"/>.
    /// </summary>
    /// <param name="musicBrainzReleaseGroupId">The MusicBrainz identifier of the release group.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The release group, or <see langword="null"/> when no release group was found.</returns>
    public Task<MusicBrainzReleaseGroupResponse?> GetReleaseGroupAsync(Guid musicBrainzReleaseGroupId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<MusicBrainzReleaseGroupResponse>(
            $"release-group/{musicBrainzReleaseGroupId:D}?inc=artist-credits+tags+genres+ratings&fmt=json", cancellationToken);
    }

    /// <summary>
    /// Searches MusicBrainz for release groups matching the provided query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of matching release groups.</returns>
    public async Task<IReadOnlyList<MusicBrainzReleaseGroupResponse>> SearchReleaseGroupsAsync(string query, int limit, CancellationToken cancellationToken)
    {
        MusicBrainzReleaseGroupSearchResponse? response = await GetJsonOrNullAsync<MusicBrainzReleaseGroupSearchResponse>(
            $"release-group?query={Uri.EscapeDataString(query)}&limit={Math.Clamp(limit, 1, 100).ToString(CultureInfo.InvariantCulture)}&fmt=json",
            cancellationToken).ConfigureAwait(false);
        return response?.ReleaseGroups ?? [];
    }

    /// <summary>
    /// Looks up the release identified by <paramref name="musicBrainzReleaseId"/>.
    /// </summary>
    /// <param name="musicBrainzReleaseId">The MusicBrainz identifier of the release.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The release, or <see langword="null"/> when no release was found.</returns>
    public Task<MusicBrainzReleaseResponse?> GetReleaseAsync(Guid musicBrainzReleaseId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<MusicBrainzReleaseResponse>(
            $"release/{musicBrainzReleaseId:D}?inc=artist-credits+labels+release-groups+media+tags+genres+artist-rels&fmt=json", cancellationToken);
    }

    /// <summary>
    /// Browses the releases that belong to the release group identified by <paramref name="musicBrainzReleaseGroupId"/>.
    /// </summary>
    /// <param name="musicBrainzReleaseGroupId">The MusicBrainz identifier of the release group.</param>
    /// <param name="limit">The maximum number of releases to return.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of releases of the release group.</returns>
    public async Task<IReadOnlyList<MusicBrainzReleaseResponse>> BrowseReleasesByReleaseGroupAsync(Guid musicBrainzReleaseGroupId, int limit, CancellationToken cancellationToken)
    {
        MusicBrainzReleaseBrowseResponse? response = await GetJsonOrNullAsync<MusicBrainzReleaseBrowseResponse>(
            $"release?release-group={musicBrainzReleaseGroupId:D}&inc=artist-credits+labels+media&limit={Math.Clamp(limit, 1, 100).ToString(CultureInfo.InvariantCulture)}&fmt=json",
            cancellationToken).ConfigureAwait(false);
        return response?.Releases ?? [];
    }

    /// <summary>
    /// Looks up the recording identified by <paramref name="musicBrainzRecordingId"/>.
    /// </summary>
    /// <param name="musicBrainzRecordingId">The MusicBrainz identifier of the recording.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The recording, or <see langword="null"/> when no recording was found.</returns>
    public Task<MusicBrainzRecordingResponse?> GetRecordingAsync(Guid musicBrainzRecordingId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<MusicBrainzRecordingResponse>(
            $"recording/{musicBrainzRecordingId:D}?inc=artist-credits+isrcs+releases+tags+genres+work-rels+artist-rels+ratings&fmt=json", cancellationToken);
    }

    /// <summary>
    /// Searches MusicBrainz for recordings matching the provided query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of matching recordings.</returns>
    public async Task<IReadOnlyList<MusicBrainzRecordingResponse>> SearchRecordingsAsync(string query, int limit, CancellationToken cancellationToken)
    {
        MusicBrainzRecordingSearchResponse? response = await GetJsonOrNullAsync<MusicBrainzRecordingSearchResponse>(
            $"recording?query={Uri.EscapeDataString(query)}&limit={Math.Clamp(limit, 1, 100).ToString(CultureInfo.InvariantCulture)}&fmt=json",
            cancellationToken).ConfigureAwait(false);
        return response?.Recordings ?? [];
    }

    /// <summary>
    /// Looks up the recordings that carry the provided ISRC.
    /// </summary>
    /// <param name="isrc">The ISRC to look up.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of recordings that carry the ISRC.</returns>
    public async Task<IReadOnlyList<MusicBrainzRecordingResponse>> GetRecordingsByIsrcAsync(string isrc, CancellationToken cancellationToken)
    {
        MusicBrainzRecordingListResponse? response = await GetJsonOrNullAsync<MusicBrainzRecordingListResponse>(
            $"isrc/{Uri.EscapeDataString(isrc)}?inc=artist-credits+isrcs+work-rels+artist-rels&fmt=json", cancellationToken).ConfigureAwait(false);
        return response?.Recordings ?? [];
    }

    /// <summary>
    /// Looks up the work identified by <paramref name="musicBrainzWorkId"/>, together with its credited contributors.
    /// </summary>
    /// <param name="musicBrainzWorkId">The MusicBrainz identifier of the work.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The work, or <see langword="null"/> when no work was found.</returns>
    public Task<MusicBrainzWorkResponse?> GetWorkAsync(Guid musicBrainzWorkId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<MusicBrainzWorkResponse>(
            $"work/{musicBrainzWorkId:D}?inc=artist-rels+tags+genres&fmt=json", cancellationToken);
    }

    /// <summary>
    /// Looks up the release identified by <paramref name="musicBrainzReleaseId"/>, together with its media, tracks, recordings and all the data needed
    /// to enrich those tracks, so that every track of the release is enriched from a single request.
    /// </summary>
    /// <param name="musicBrainzReleaseId">The MusicBrainz identifier of the release.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The release, with its media, tracks and recordings, or <see langword="null"/> when no release was found.</returns>
    public Task<MusicBrainzReleaseResponse?> GetReleaseWithRecordingsAsync(Guid musicBrainzReleaseId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<MusicBrainzReleaseResponse>(
            $"release/{musicBrainzReleaseId:D}?inc=recordings+isrcs+artist-credits+tags+genres+ratings+work-rels+recording-level-rels+work-level-rels+artist-rels+release-groups+labels+media&fmt=json", cancellationToken);
    }

    /// <summary>
    /// Gets the JSON resource at the given relative URL and deserializes it into <typeparamref name="TModel"/>.
    /// </summary>
    /// <typeparam name="TModel">The type to deserialize the JSON response into.</typeparam>
    /// <param name="relativeUrl">The URL of the resource, relative to the base address.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized resource, or <see langword="null"/> when the resource was not found.</returns>
    /// <remarks>Transient failures and rate limiting are handled by retrying the request up to three times.</remarks>
    private async Task<TModel?> GetJsonOrNullAsync<TModel>(string relativeUrl, CancellationToken cancellationToken)
    {
        MusicBrainzSettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);
        string cacheKey = string.Concat(settings.BaseUrl, relativeUrl);
        if (_responseCache.TryGet(cacheKey, out string? cachedJson) && cachedJson is not null)
            return JsonSerializer.Deserialize<TModel>(cachedJson, s_serializerOptions);

        using HttpResponseMessage response = await SendAsync(settings, relativeUrl, cancellationToken).ConfigureAwait(false);
        // A 404 means the entity does not exist in MusicBrainz, which is a normal miss rather than an error, so it is reported as no result.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _responseCache.Set(cacheKey, json);
        return JsonSerializer.Deserialize<TModel>(json, s_serializerOptions);
    }

    /// <summary>
    /// Sends a GET request to the MusicBrainz web service, throttled to respect the configured minimum request interval unless the configured
    /// base URL is not the real MusicBrainz service, in which case no rate limit is imposed. Transient failures, rate limiting and transport
    /// exceptions are retried, preferring the delay requested by the web service when it is provided.
    /// </summary>
    /// <param name="settings">The settings that configure the MusicBrainz web service requests.</param>
    /// <param name="relativeUrl">The URL of the resource, relative to the base address.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The HTTP response message for the request.</returns>
    private async Task<HttpResponseMessage> SendAsync(MusicBrainzSettingsDto settings, string relativeUrl, CancellationToken cancellationToken)
    {
        EnsureRequestHeaders(settings);
        // The rate limit is enforced by the MusicBrainz web service itself, so it is only observed when the requests actually reach it; a self hosted
        // mirror, or any other configured base URL that is not the real service, is not limited by this process.
        bool shouldThrottle = IsRealMusicBrainzService(settings.BaseUrl);

        // A fresh request is created for every attempt, because an HTTP request cannot be sent more than once, and the throttle is applied to every
        // attempt, so that the retries of a failed request cannot burst past the rate limit of the real MusicBrainz web service.
        for (int attempt = 1; ; attempt++)
        {
            if (shouldThrottle)
                await _requestThrottle.WaitAsync(settings.MinimumRequestInterval, cancellationToken).ConfigureAwait(false);

            bool isLastAttempt = attempt >= MAXIMUM_ATTEMPT_COUNT;
            HttpResponseMessage response;
            try
            {
                HttpRequestMessage request = new(HttpMethod.Get, BuildRequestUri(settings.BaseUrl, relativeUrl));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException) when (!isLastAttempt)
            {
                await Task.Delay(GetRetryDelay(attempt, null), cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (!isLastAttempt && (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500))
            {
                TimeSpan retryDelay = GetRetryDelay(attempt, response);
                // The response of a failed attempt is not returned, so it must be disposed here to release its connection.
                response.Dispose();
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                continue;
            }

            return response;
        }
    }

    /// <summary>
    /// Gets the delay to wait before the next attempt, preferring the delay requested by the MusicBrainz web service when it is provided.
    /// </summary>
    /// <param name="attempt">The one based number of the attempt that just failed.</param>
    /// <param name="response">The response of the failed attempt, when the failure was a response rather than a transport exception.</param>
    /// <returns>The delay to wait before the next attempt.</returns>
    private static TimeSpan GetRetryDelay(int attempt, HttpResponseMessage? response)
    {
        if (response?.Headers.RetryAfter is { } retryAfter)
        {
            if (retryAfter.Delta is TimeSpan delta)
                return delta;
            if (retryAfter.Date is DateTimeOffset date)
            {
                TimeSpan wait = date - DateTimeOffset.UtcNow;
                if (wait > TimeSpan.Zero)
                    return wait;
            }
        }
        return TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt));
    }

    /// <summary>
    /// Determines whether the provided base URL points to the real MusicBrainz web service, which is the only one whose rate limit is observed.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MusicBrainz web service the requests are sent to.</param>
    /// <returns><see langword="true"/> when the base URL points to the real MusicBrainz web service, otherwise <see langword="false"/>.</returns>
    private static bool IsRealMusicBrainzService(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri))
            return false;
        return baseUri.Host.Equals("musicbrainz.org", StringComparison.OrdinalIgnoreCase)
            || baseUri.Host.EndsWith(".musicbrainz.org", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Applies the request headers configured in the <paramref name="settings"/> onto the underlying HTTP client, unless they are already set.
    /// </summary>
    /// <param name="settings">The settings that configure the MusicBrainz web service requests.</param>
    private void EnsureRequestHeaders(MusicBrainzSettingsDto settings)
    {
        // The headers live on the underlying HttpClient, which is reused across the calls of this client, so they are applied only once.
        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            string userAgent = string.IsNullOrWhiteSpace(settings.ContactEmail)
                ? settings.UserAgent
                : $"{settings.UserAgent} ( {settings.ContactEmail} )";
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }
    }

    /// <summary>
    /// Combines the configured base URL with the given relative URL into the absolute URI of a request.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MusicBrainz web service the requests are sent to.</param>
    /// <param name="relativeUrl">The URL of the resource, relative to the base URL.</param>
    /// <returns>The absolute URI of the request.</returns>
    private static Uri BuildRequestUri(string baseUrl, string relativeUrl)
    {
        string normalizedBaseUrl = baseUrl.EndsWith('/') ? baseUrl : string.Concat(baseUrl, "/");
        return new Uri(new Uri(normalizedBaseUrl, UriKind.Absolute), relativeUrl);
    }
}
