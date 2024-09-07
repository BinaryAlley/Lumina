#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Models.FileSystem;
#endregion

namespace Lumina.Presentation.Web.Common.Api;

/// <summary>
/// Interface for typed clients for API access.
/// </summary>
public interface IApiHttpClient
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the GET request.</returns>
    Task<TResponse> GetAsync<TResponse>(string endpoint, string? token = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A model containing the deserialized blob.</returns>
    Task<BlobDataModel> GetBlobAsync(string endpoint, string? token = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="token">The token used for authentication with the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the DELETE request.</returns>
    Task<TResponse> DeleteAsync<TResponse>(string endpoint, string? token = null, CancellationToken cancellationToken = default);

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
    Task<TResponse> PutAsync<TResponse, TModel>(string endpoint, TModel data, string? token = null, CancellationToken cancellationToken = default);

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
    Task<TResponse> PostAsync<TResponse, TModel>(string endpoint, TModel data, string? token = null, CancellationToken cancellationToken = default);
    #endregion
}