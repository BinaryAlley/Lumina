#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Api;

/// <summary>
/// Typed client for streaming API access. Unlike <see cref="ApiHttpClient"/>, it never buffers a response in memory and is
/// intentionally free of any retry or circuit breaker policy, because a stream is meant to be handed to the client as a whole.
/// </summary>
public class StreamingApiHttpClient : IStreamingApiHttpClient
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly HttpContext? _httpContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamingApiHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">Injected HttpClient for interacting with the API.</param>
    /// <param name="httpContextAccessor">Injected service for providing the current HTTP context.</param>
    /// <param name="serverConfigurationOptions">Injected server configuration application settings.</param>
    public StreamingApiHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions)
    {
        _httpClient = httpClient;
        _httpContext = httpContextAccessor.HttpContext;
        // Read the API server configuration values from the configuration, and assign them to the injected client.
        ServerConfigurationDto serverConfiguration = serverConfigurationOptions.Value;
        httpClient.BaseAddress = new Uri($"{serverConfiguration.BaseAddress}:{serverConfiguration.Port}/api/v{serverConfiguration.ApiVersion}/");
        // A streamed response stays open for as long as the client keeps reading it, so the timeout is generously large, but it is bounded:
        // an upstream that accepts the request and never answers must not hold a connection and a stream permit open forever. No track, and
        // no movie, is longer than this.
        httpClient.Timeout = TimeSpan.FromHours(12);
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the response
    /// as a stream, without ever buffering its content in memory.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="requestHeaders">The range and conditional headers of the incoming request to forward, so that the API, which is the only party that knows the entity tag and the modification time of the resource, validates them itself.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The streamed response of the API request, which the caller must dispose once it has finished copying it.</returns>
    /// <exception cref="ApiException">Thrown if the response is not successful, with the <see cref="ProblemDetailsDto"/> if available.</exception>
    public async Task<ApiStreamResult> GetStreamAsync(string endpoint, IReadOnlyDictionary<string, string> requestHeaders, CancellationToken cancellationToken = default)
    {
        using (HttpRequestMessage request = new(HttpMethod.Get, endpoint))
        {
            AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
            if (authenticationHeader is not null)
                request.Headers.Authorization = authenticationHeader;
            // The range and the conditional headers are forwarded as they are, so that the API, and never this client, decides what a partial response contains.
            foreach (KeyValuePair<string, string> header in requestHeaders)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Only the headers of the response are awaited, so that its content is streamed to the client instead of being copied into memory first.
            HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            // A not modified response and a range not satisfiable response are valid outcomes of a conditional or ranged request, and their
            // headers (the entity tag, the last modified date, the content range) are the whole answer, so they are passed through instead of
            // being turned into an error, which would drop the very headers the caller needs to revalidate or recover.
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotModified && response.StatusCode != HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                HttpStatusCode statusCode = response.StatusCode;
                string content;
                try
                {
                    content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    // The response is turned into an exception, so it is disposed even when reading its error body fails, otherwise its connection would leak.
                    response.Dispose();
                }
                ProblemDetailsDto? problemDetails = null;
                try
                {
                    problemDetails = JsonSerializer.Deserialize<ProblemDetailsDto>(content, s_jsonOptions);
                }
                catch { /* If we can't deserialize to ProblemDetails, we'll just use the status code. */ }
                throw new ApiException(problemDetails, statusCode, request.RequestUri?.AbsolutePath);
            }
            try
            {
                return await ApiStreamResult.CreateAsync(response, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // The response was already received, so it must be disposed if its content stream could not be obtained, otherwise its connection would leak.
                response.Dispose();
                throw;
            }
        }
    }

    /// <summary>
    /// Retrieves the authentication header for the current user, if available.
    /// </summary>
    /// <returns>
    /// An <see cref="AuthenticationHeaderValue"/> with a Bearer token if the user's token is found, <see langword="null"/> otherwise.
    /// </returns>
    private AuthenticationHeaderValue? GetAuthenticationHeader()
    {
        string? token = _httpContext?.User?.FindFirst("Token")?.Value;
        return !string.IsNullOrEmpty(token) ? new AuthenticationHeaderValue("Bearer", token) : null;
    }
}
