#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using Lumina.Plugins.CoverArtArchive.Common.Models.DTO.Settings;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core.Api;

/// <summary>
/// HTTP client that calls the Cover Art Archive API and deserializes its JSON responses into typed response models.
/// </summary>
internal sealed class CoverArtArchiveHttpClient
{
    private const string BASE_URL = "https://coverartarchive.org/";

    // Transient failures and rate limiting are retried with an exponential backoff, so a single hiccup does not mark an item as failed to enrich.
    private const int MAXIMUM_ATTEMPT_COUNT = 3;

    // The metadata endpoints of the Cover Art Archive answer with a temporary redirect to the Internet Archive, so a bounded number of redirects is followed.
    private const int MAXIMUM_REDIRECT_COUNT = 3;

    // The delay requested by a rate limited response is honored, but only up to this many seconds, so a misbehaving server cannot stall the scan.
    private const double MAXIMUM_RETRY_DELAY_SECONDS = 60;

    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly CoverArtArchiveSettingsProvider _settingsProvider;
    private readonly CoverArtArchiveRequestThrottle _requestThrottle;
    private readonly CoverArtArchiveResponseCache _responseCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoverArtArchiveHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to send requests to the Cover Art Archive API.</param>
    /// <param name="settingsProvider">The provider of the settings that configure the Cover Art Archive API requests.</param>
    /// <param name="requestThrottle">The process-wide throttle that spaces out the requests sent to the Cover Art Archive API.</param>
    /// <param name="responseCache">The process-wide cache that avoids fetching the same entity more than once during a run.</param>
    public CoverArtArchiveHttpClient(HttpClient httpClient, CoverArtArchiveSettingsProvider settingsProvider, CoverArtArchiveRequestThrottle requestThrottle, CoverArtArchiveResponseCache responseCache)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        _requestThrottle = requestThrottle;
        _responseCache = responseCache;
    }

    /// <summary>
    /// Gets the artwork of the release identified by <paramref name="musicBrainzReleaseId"/>.
    /// </summary>
    /// <param name="musicBrainzReleaseId">The MusicBrainz identifier of the release.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artwork of the release, or <see langword="null"/> when the release has no artwork.</returns>
    public Task<CoverArtArchiveArtworkResponse?> GetReleaseArtworkAsync(Guid musicBrainzReleaseId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<CoverArtArchiveArtworkResponse>($"release/{musicBrainzReleaseId:D}", cancellationToken);
    }

    /// <summary>
    /// Gets the artwork of the release group identified by <paramref name="musicBrainzReleaseGroupId"/>.
    /// </summary>
    /// <param name="musicBrainzReleaseGroupId">The MusicBrainz identifier of the release group.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artwork of the release group, or <see langword="null"/> when the release group has no artwork.</returns>
    public Task<CoverArtArchiveArtworkResponse?> GetReleaseGroupArtworkAsync(Guid musicBrainzReleaseGroupId, CancellationToken cancellationToken)
    {
        return GetJsonOrNullAsync<CoverArtArchiveArtworkResponse>($"release-group/{musicBrainzReleaseGroupId:D}", cancellationToken);
    }

    /// <summary>
    /// Gets the JSON resource at the given relative URL and deserializes it into <typeparamref name="TModel"/>.
    /// </summary>
    /// <typeparam name="TModel">The type to deserialize the JSON response into.</typeparam>
    /// <param name="relativeUrl">The URL of the resource, relative to the base address.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized resource, or <see langword="null"/> when the resource was not found.</returns>
    private async Task<TModel?> GetJsonOrNullAsync<TModel>(string relativeUrl, CancellationToken cancellationToken)
    {
        // A cached response, including a confirmed absence of artwork, removes the repeated lookups of the same entity, like the release group that the
        // releases of an edition heavy library share, or the same library scanned again while the entry is still valid.
        if (_responseCache.TryGet(relativeUrl, out string? cachedJson))
            return string.IsNullOrEmpty(cachedJson) ? default : JsonSerializer.Deserialize<TModel>(cachedJson, s_serializerOptions);

        CoverArtArchiveSettingsDto settings = await _settingsProvider.GetAsync(cancellationToken).ConfigureAwait(false);

        using HttpResponseMessage response = await SendAsync(settings, relativeUrl, cancellationToken).ConfigureAwait(false);
        // A 404 means the entity exists but has no artwork, which is a normal miss rather than an error, so it is reported as no result and cached as such.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _responseCache.Set(relativeUrl, string.Empty);
            return default;
        }
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        TModel? model = JsonSerializer.Deserialize<TModel>(json, s_serializerOptions);
        _responseCache.Set(relativeUrl, json);
        return model;
    }

    /// <summary>
    /// Sends a GET request to the Cover Art Archive API, throttled to respect the configured minimum request interval. Transient failures and
    /// rate limiting are retried, so a single hiccup does not surface as a failed resolution.
    /// </summary>
    /// <param name="settings">The settings that configure the Cover Art Archive API requests.</param>
    /// <param name="relativeUrl">The URL of the resource, relative to the base address.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The HTTP response message for the request.</returns>
    private async Task<HttpResponseMessage> SendAsync(CoverArtArchiveSettingsDto settings, string relativeUrl, CancellationToken cancellationToken)
    {
        EnsureRequestHeaders(settings);

        Uri requestUri = new(new Uri(BASE_URL, UriKind.Absolute), relativeUrl);

        // The metadata endpoints answer with a temporary redirect to the Internet Archive, so the redirect is followed explicitly, which keeps the
        // automatic redirects disabled and confines the following to the hosts the Cover Art Archive is allowed to delegate its responses to.
        for (int redirectCount = 0; redirectCount <= MAXIMUM_REDIRECT_COUNT; redirectCount++)
        {
            HttpResponseMessage response = await SendWithRetriesAsync(settings, requestUri, cancellationToken).ConfigureAwait(false);

            if (!IsRedirect(response.StatusCode) || response.Headers.Location is null)
                return response;

            Uri? redirectUri = ResolveRedirect(requestUri, response.Headers.Location);
            // The response of a redirect is not returned, so it must be disposed here to release its connection.
            response.Dispose();

            // A redirect that is not an absolute HTTPS URL, or that points to a host other than the Cover Art Archive or the Internet Archive, is refused,
            // so a compromised or misbehaving endpoint cannot redirect the request to any other host.
            if (redirectUri is null || redirectUri.Scheme != Uri.UriSchemeHttps || !CoverArtArchiveHosts.IsAllowed(redirectUri.Host))
                throw new HttpRequestException($"The Cover Art Archive redirected the request to the disallowed host '{redirectUri?.Host}'.");

            requestUri = redirectUri;
        }

        throw new HttpRequestException("The Cover Art Archive redirected the request too many times.");
    }

    /// <summary>
    /// Sends a GET request to <paramref name="requestUri"/>, throttled to respect the configured minimum request interval.
    /// Transient failures and rate limiting are retried, so a single hiccup does not surface as a failed resolution.
    /// </summary>
    /// <param name="settings">The settings that configure the Cover Art Archive API requests.</param>
    /// <param name="requestUri">The absolute URI of the resource to request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The HTTP response message for the request.</returns>
    private async Task<HttpResponseMessage> SendWithRetriesAsync(CoverArtArchiveSettingsDto settings, Uri requestUri, CancellationToken cancellationToken)
    {
        // A fresh request is created for every attempt, because an HTTP request cannot be sent more than once, and the throttle is applied to every
        // attempt, so that the retries of a failed request cannot burst past the rate limit of the public service.
        for (int attempt = 1; ; attempt++)
        {
            await _requestThrottle.WaitAsync(settings.MinimumRequestInterval, cancellationToken).ConfigureAwait(false);

            bool isLastAttempt = attempt >= MAXIMUM_ATTEMPT_COUNT;
            HttpResponseMessage response;
            try
            {
                HttpRequestMessage request = new(HttpMethod.Get, requestUri);
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
                // The delay is read before the response is disposed, because a rate limited response tells the caller how long to wait.
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
    /// Determines whether the provided status code is a redirect.
    /// </summary>
    /// <param name="statusCode">The status code to check.</param>
    /// <returns><see langword="true"/> when the status code is a redirect, otherwise <see langword="false"/>.</returns>
    private static bool IsRedirect(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;
    }

    /// <summary>
    /// Resolves the location of a redirect against the URI of the request it was returned for.
    /// </summary>
    /// <param name="requestUri">The URI of the request the redirect was returned for.</param>
    /// <param name="location">The location of the redirect.</param>
    /// <returns>The absolute URI of the redirect, or <see langword="null"/> when it could not be resolved.</returns>
    private static Uri? ResolveRedirect(Uri requestUri, Uri location)
    {
        if (location.IsAbsoluteUri)
            return location;
        return Uri.TryCreate(requestUri, location, out Uri? resolvedUri) ? resolvedUri : null;
    }

    /// <summary>
    /// Gets the delay to wait before the next attempt, preferring the delay requested by the Cover Art Archive when it is provided.
    /// </summary>
    /// <param name="attempt">The one based number of the attempt that just failed.</param>
    /// <param name="response">The response of the failed attempt, when the failure was a response rather than a transport exception.</param>
    /// <returns>The delay to wait before the next attempt.</returns>
    internal static TimeSpan GetRetryDelay(int attempt, HttpResponseMessage? response)
    {
        // The delay requested by the response is preferred when it is provided, so the rate limit policy of the Cover Art Archive is honored.
        if (response?.Headers.RetryAfter is { } retryAfter)
        {
            TimeSpan? requestedDelay = retryAfter.Delta;
            if (requestedDelay is null && retryAfter.Date is DateTimeOffset date)
            {
                TimeSpan wait = date - DateTimeOffset.UtcNow;
                if (wait > TimeSpan.Zero)
                    requestedDelay = wait;
            }
            if (requestedDelay is TimeSpan delay)
            {
                // A misbehaving server could ask for an extremely long delay, which would stall the scan until it is cancelled, so the wait is capped.
                TimeSpan maximumDelay = TimeSpan.FromSeconds(MAXIMUM_RETRY_DELAY_SECONDS);
                return delay > maximumDelay ? maximumDelay : delay;
            }
        }
        return TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt));
    }

    /// <summary>
    /// Applies the request headers configured in the <paramref name="settings"/> onto the underlying HTTP client, unless they are already set.
    /// </summary>
    /// <param name="settings">The settings that configure the Cover Art Archive API requests.</param>
    private void EnsureRequestHeaders(CoverArtArchiveSettingsDto settings)
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
}
