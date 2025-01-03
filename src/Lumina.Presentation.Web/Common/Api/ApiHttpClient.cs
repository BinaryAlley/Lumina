#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Models.Common;
using Lumina.Presentation.Web.Common.Models.Configuration;
using Lumina.Presentation.Web.Common.Models.FileSystemManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Api;

/// <summary>
/// Typed client for API access.
/// </summary>
public class ApiHttpClient : IApiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpContext? _httpContext;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">Injected HttpClient for interacting with the API.</param>
    /// <param name="httpContextAccessor">Injected service for providing the current HTTP context.</param>
    /// <param name="serverConfigurationOptions">Injected server configuration application settings.</param>
    public ApiHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptionsSnapshot<ServerConfigurationModel> serverConfigurationOptions)
    {
        _httpClient = httpClient;
        _httpContext = httpContextAccessor.HttpContext;
        // read the API server configuration values from the configuration, and assign them to the injected client
        ServerConfigurationModel serverConfigurationModel = serverConfigurationOptions.Value;
        httpClient.BaseAddress = new Uri($"{serverConfigurationModel.BaseAddress}:{serverConfigurationModel.Port}/api/v{serverConfigurationModel.ApiVersion}/");

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the GET request.</returns>
    public async Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
        if (authenticationHeader is not null)
            request.Headers.Authorization = authenticationHeader;
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result as a streamable response.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> object, which allows for asynchronous iteration over the deserialized items.</returns>
    public async IAsyncEnumerable<TResponse?> GetAsyncEnumerable<TResponse>(string endpoint, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
        if (authenticationHeader is not null)
            request.Headers.Authorization = authenticationHeader;
        // send the request and expect only headers initially - this prevents the client from buffering the entire response
        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        // Deserialize the JSON content asynchronously as an enumerable of TResponse items
        await foreach (TResponse? item in JsonSerializer.DeserializeAsyncEnumerable<TResponse>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            yield return item;
        }
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A model containing the deserialized blob.</returns>
    public async Task<BlobDataModel> GetBlobAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
        if (authenticationHeader is not null)
            request.Headers.Authorization = authenticationHeader;
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            // read the content as a string, for error messages
            string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            ProblemDetailsModel? problemDetails = null;
            try
            {
                problemDetails = JsonSerializer.Deserialize<ProblemDetailsModel>(content, _jsonOptions);
            }
            catch { /* if we can't deserialize to ProblemDetails, we'll just use the status code */ }
            throw new ApiException(problemDetails, response.StatusCode);
        }
        // if the response is successful, read it as a byte array
        byte[] responseContent = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        string contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        return new BlobDataModel { Data = responseContent, ContentType = contentType };
    }

    /// <summary>
    /// Sends a DELETE request to the specified <paramref name="endpoint"/> as an asynchronous operation.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, endpoint);
        AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
        if (authenticationHeader is not null)
            request.Headers.Authorization = authenticationHeader;
        //return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
        // send the HTTP request and read the response
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            ProblemDetailsModel? problemDetails = null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            // attempt to deserialize the response content to ProblemDetailsModel if the request fails
            try
            {
                problemDetails = JsonSerializer.Deserialize<ProblemDetailsModel>(content, _jsonOptions);
            }
            catch { /* if we can't deserialize to ProblemDetails, we'll just use the status code */ }
            throw new ApiException(problemDetails, response.StatusCode);
        }
    }

    /// <summary>
    /// Sends a PUT request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and send to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the PUT request.</returns>
    public async Task<TResponse> PutAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Put, endpoint);
        AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
        if (authenticationHeader is not null)
            request.Headers.Authorization = authenticationHeader;
        request.Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a POST request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the POST request.</returns>
    public async Task<TResponse> PostAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
        AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
        if (authenticationHeader is not null)
            request.Headers.Authorization = authenticationHeader;
        request.Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends an HTTP request and deserializes the response into the specified type if the request is successful.
    /// If the request fails, it attempts to deserialize the response content into a <see cref="ProblemDetailsModel"/> and throws an <see cref="ApiException"/>.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to be sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response content as an object of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ApiException">Thrown if the response is not successful, with the <see cref="ProblemDetailsModel"/> if available.</exception>
    private async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // send the HTTP request and read the response
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        // if the response is successful, deserialize the content to TResponse and return it
        if (response.IsSuccessStatusCode)
        {
            // handle empty content case
            if (string.IsNullOrEmpty(content))
                return default!;
            return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions)!;
        }
        else
        {
            ProblemDetailsModel? problemDetails = null;
            // attempt to deserialize the response content to ProblemDetailsModel if the request fails
            try
            {
                problemDetails = JsonSerializer.Deserialize<ProblemDetailsModel>(content, _jsonOptions);
            }
            catch { /* if we can't deserialize to ProblemDetails, we'll just use the status code */ }
            throw new ApiException(problemDetails, response.StatusCode);
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
