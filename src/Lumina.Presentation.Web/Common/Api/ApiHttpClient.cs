#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Models.Common;
using Lumina.Presentation.Web.Common.Models.Configuration;
using Lumina.Presentation.Web.Common.Models.FileSystem;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        var serverConfigurationModel = serverConfigurationOptions.Value;
        httpClient.BaseAddress = new Uri(serverConfigurationModel.BaseAddress + ":" + serverConfigurationModel.Port + "/api/v1/");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await SendRequestAsync<TResponse>(request, cancellationToken);
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
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // read the content as a string, for error messages
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            ProblemDetailsModel? problemDetails = null;
            try
            {
                problemDetails = JsonSerializer.Deserialize<ProblemDetailsModel>(content, _jsonOptions);
            }
            catch { /* if we can't deserialize to ProblemDetails, we'll just use the status code */ }
            throw new ApiException(problemDetails, response.StatusCode);
        }
        // if the response is successful, read it as a byte array
        byte[] responseContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
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
        using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await SendRequestAsync<TResponse>(request, cancellationToken);
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
        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        return await SendRequestAsync<TResponse>(request, cancellationToken);
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
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
        return await SendRequestAsync<TResponse>(request, cancellationToken);
    }
        
    /// <summary>
    /// Sends an HTTP request and deserializes the response into the specified type if the request is successful.
    /// If the request fails, it attempts to deserialize the response content into a <see cref="ProblemDetailsModel"/> and throws an <see cref="ApiException"/>.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to be sent.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response content as an object of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ApiException">Thrown if the response is not successful, with the <see cref="ProblemDetailsModel"/> if available.</exception>
    private async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // send the HTTP request and read the response
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
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