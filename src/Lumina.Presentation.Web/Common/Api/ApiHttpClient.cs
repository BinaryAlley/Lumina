#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Models.Common;
using Lumina.Presentation.Web.Common.Models.Configuration;
using Lumina.Presentation.Web.Common.Models.FileManagement;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">Injected HttpClient for interacting with the API.</param>
    /// <param name="serverConfigurationOptions">Injected server configuration application settings.</param>
    public ApiHttpClient(HttpClient httpClient, IOptionsSnapshot<ServerConfigurationModel> serverConfigurationOptions)
    {
        _httpClient = httpClient;
        // read the API server configuration values from the configuration, and assign them to the injected client
        ServerConfigurationModel serverConfigurationModel = serverConfigurationOptions.Value;
        httpClient.BaseAddress = new Uri($"{serverConfigurationModel.BaseAddress}:{serverConfigurationModel.Port}/api/v{serverConfigurationModel.ApiVersion}/");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the GET request.</returns>
    public async Task<TResponse> GetAsync<TResponse>(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result as a streamable response.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> object, which allows for asynchronous iteration over the deserialized items.</returns>
    public async IAsyncEnumerable<TResponse?> GetAsyncEnumerable<TResponse>(string endpoint, string? token = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        // HttpClient is implemented differently in Blazor WASM, because there are no sockets in the browser, so its BrowserHttpHandler is implemented on top of Fetch API,
        // which can provide response content in one of two forms: BrowserHttpContent, which is based on arrayBuffer method (this means that it will always read the response stream to its completion,
        // before making the content available), and StreamContent that is wrapping WasmHttpReadStream, which is based on readable streams (this one allows for reading response as it comes).
        // WasmHttpReadStream can only work when WebAssemblyEnableStreamingResponse is enabled on request, and only on browsers that support it
        request.SetBrowserResponseStreamingEnabled(true);

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // send the request and expect only headers initially - this prevents the client from buffering the entire response
        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using System.IO.Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
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
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A model containing the deserialized blob.</returns>
    public async Task<BlobDataModel> GetBlobAsync(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
    /// Sends a DELETE request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the DELETE request.</returns>
    public async Task<TResponse> DeleteAsync<TResponse>(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, endpoint);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a PUT request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and send to the API.</param>
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the PUT request.</returns>
    public async Task<TResponse> PutAsync<TResponse, TModel>(string endpoint, TModel data, string? token = null, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Put, endpoint);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the POST request.</returns>
    public async Task<TResponse> PostAsync<TResponse, TModel>(string endpoint, TModel data, string? token = null, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
            return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions)!;
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
    #endregion
}