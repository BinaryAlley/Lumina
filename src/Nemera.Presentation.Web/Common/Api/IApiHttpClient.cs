#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Api;

/// <summary>
/// Interface for typed clients for API access.
/// </summary>
public interface IApiHttpClient
{
    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the GET request.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result as a streamable response.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> object, which allows for asynchronous iteration over the deserialized items.</returns>
    IAsyncEnumerable<TResponse?> GetAsyncEnumerable<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A model containing the raw blob bytes and their content type.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task<BlobDataDto> GetBlobAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task PutIgnoringResponseAsync<TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default);

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
    Task<TResponse> PutAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request with a multipart form containing a single file to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="fileStream">The stream of the file to upload, or <see langword="null"/> to send a multipart form without any file.</param>
    /// <param name="fileName">The name of the file to upload, or <see langword="null"/> when no file is sent.</param>
    /// <param name="fieldName">The name of the form field carrying the file.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the PUT request.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task<TResponse> PutMultipartAsync<TResponse>(string endpoint, Stream? fileStream, string? fileName, string fieldName, CancellationToken cancellationToken = default);

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
    Task<TResponse> PostMultipartAsync<TResponse>(string endpoint, Stream fileStream, string fileName, string fieldName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request without a payload to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task PostIgnoringResponseAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request without a payload to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the POST request.</returns>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task<TResponse> PostAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request to the specified <paramref name="endpoint"/> as an asynchronous operation, discarding the response content.
    /// </summary>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <exception cref="ApiException">Thrown when the API returns an unsuccessful response.</exception>
    Task PostIgnoringResponseAsync<TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default);

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
    Task<TResponse> PostAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default);
}
