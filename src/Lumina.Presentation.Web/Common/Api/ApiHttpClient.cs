#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Exceptions;
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
    public ApiHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions)
    {
        _httpClient = httpClient;
        _httpContext = httpContextAccessor.HttpContext;
        // Read the API server configuration values from the configuration, and assign them to the injected client.
        ServerConfigurationDto serverConfiguration = serverConfigurationOptions.Value;
        httpClient.BaseAddress = new Uri($"{serverConfiguration.BaseAddress}:{serverConfiguration.Port}/api/v{serverConfiguration.ApiVersion}/");

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
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, endpoint);
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
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, endpoint);
        // Send the request and expect only headers initially - this prevents the client from buffering the entire response.
        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        // Deserialize the JSON content asynchronously as an enumerable of TResponse items.
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
    /// <returns>A model containing the raw blob bytes and their content type.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task<BlobDataDto> GetBlobAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, endpoint);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw CreateApiException(content, response, request);
        }
        // If the response is successful, read it as a byte array.
        byte[] responseContent = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        string contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        return new BlobDataDto { Data = responseContent, ContentType = contentType };
    }

    /// <summary>
    /// Sends a DELETE request to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Delete, endpoint);
        await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a PUT request to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task PutIgnoringResponseAsync<TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequestWithPayload(HttpMethod.Put, endpoint, data);
        await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a PUT request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the PUT request.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task<TResponse> PutAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequestWithPayload(HttpMethod.Put, endpoint, data);
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a PUT request with a multipart form containing a single file to the specified <paramref name="endpoint"/> as an asynchronous operation, and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="fileStream">The stream of the file to upload, or <see langword="null"/> to send a multipart form without any file.</param>
    /// <param name="fileName">The name of the file to upload, or <see langword="null"/> when no file is sent.</param>
    /// <param name="fieldName">The name of the form field carrying the file.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the PUT request.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task<TResponse> PutMultipartAsync<TResponse>(string endpoint, Stream? fileStream, string? fileName, string fieldName, CancellationToken cancellationToken = default)
    {
        using MultipartFormDataContent form = [];
        if (fileStream is not null)
        {
            // The file is streamed as a generic binary part, since the exact content type of the uploaded file is not known.
            StreamContent fileContent = new(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, fieldName, fileName ?? string.Empty);
        }

        using HttpRequestMessage request = CreateRequest(HttpMethod.Put, endpoint);
        request.Content = form;
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a POST request with a multipart form containing a single file to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="fileStream">The stream of the file to upload.</param>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <param name="fieldName">The name of the form field carrying the file.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the POST request.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task<TResponse> PostMultipartAsync<TResponse>(string endpoint, Stream fileStream, string fileName, string fieldName, CancellationToken cancellationToken = default)
    {
        // The file is streamed as a generic binary part, since the exact content type of the uploaded file is not known.
        using MultipartFormDataContent form = [];
        using StreamContent fileContent = new(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, fieldName, fileName);

        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, endpoint);
        request.Content = form;
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a POST request without a payload to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task PostIgnoringResponseAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, endpoint);
        await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a POST request without a payload to the specified <paramref name="endpoint"/> as an asynchronous operation, and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the POST request.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task<TResponse> PostAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, endpoint);
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a POST request to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task PostIgnoringResponseAsync<TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequestWithPayload(HttpMethod.Post, endpoint, data);
        await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
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
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    public async Task<TResponse> PostAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequestWithPayload(HttpMethod.Post, endpoint, data);
        return await SendRequestAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates an HTTP request for the specified <paramref name="method"/> and <paramref name="endpoint"/>.
    /// </summary>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <returns>The created <see cref="HttpRequestMessage"/>.</returns>
    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        HttpRequestMessage request = new(method, endpoint);
        AuthenticationHeaderValue? authenticationHeader = GetAuthenticationHeader();
        if (authenticationHeader is not null)
            request.Headers.Authorization = authenticationHeader;
        return request;
    }

    /// <summary>
    /// Creates an HTTP request for the specified <paramref name="method"/> and <paramref name="endpoint"/>, with the serialized <paramref name="data"/> as its JSON payload.
    /// </summary>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <returns>The created <see cref="HttpRequestMessage"/>.</returns>
    private HttpRequestMessage CreateRequestWithPayload<TModel>(HttpMethod method, string endpoint, TModel data)
    {
        HttpRequestMessage request = CreateRequest(method, endpoint);
        request.Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        return request;
    }

    /// <summary>
    /// Sends an HTTP request and reads the response content.
    /// If the request fails, it attempts to deserialize the response content into a <see cref="ProblemDetailsDto"/> and throws an <see cref="ApiException"/>.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to be sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The raw response content.</returns>
    /// <exception cref="ApiException">Thrown if the response is not successful, with the <see cref="ProblemDetailsDto"/> if available.</exception>
    private async Task<string> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
            return content;
        throw CreateApiException(content, response, request);
    }

    /// <summary>
    /// Sends an HTTP request and deserializes the response into the specified type if the request is successful.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to be sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response content as an object of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ApiException">Thrown if the response is not successful, with the <see cref="ProblemDetailsDto"/> if available.</exception>
    private async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string content = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        // Handle empty content case.
        if (string.IsNullOrEmpty(content))
            return default!;
        return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions)!;
    }

    /// <summary>
    /// Creates the exception thrown when an API request fails.
    /// </summary>
    /// <param name="content">The raw response content.</param>
    /// <param name="response">The failed HTTP response.</param>
    /// <param name="request">The HTTP request that failed.</param>
    /// <returns>The created <see cref="ApiException"/>.</returns>
    private ApiException CreateApiException(string content, HttpResponseMessage response, HttpRequestMessage request)
    {
        ProblemDetailsDto? problemDetails = null;
        // Attempt to deserialize the response content to ProblemDetailsDto if the request fails.
        try
        {
            problemDetails = JsonSerializer.Deserialize<ProblemDetailsDto>(content, _jsonOptions);
        }
        catch { /* If we can't deserialize to ProblemDetails, we'll just use the status code. */ }
        return new ApiException(problemDetails, response.StatusCode, request.RequestUri?.AbsolutePath);
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
