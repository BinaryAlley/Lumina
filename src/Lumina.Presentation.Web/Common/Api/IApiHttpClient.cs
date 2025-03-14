#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Models.FileSystemManagement;
using System.Collections.Generic;
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
    /// <returns>A model containing the deserialized blob.</returns>
    Task<BlobDataModel> GetBlobAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified <paramref name="endpoint"/> as an asynchronous operation.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and send to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the PUT request.</returns>
    Task<TResponse> PutAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and sent to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the POST request.</returns>
    Task<TResponse> PostAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default);
}
