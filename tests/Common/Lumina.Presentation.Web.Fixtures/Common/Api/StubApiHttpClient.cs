#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Responses.Authorization;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Api;

/// <summary>
/// Test double for the <see cref="IApiHttpClient"/> interface, allowing integration and security tests to stub the
/// remote API responses without hosting a real backend.
/// </summary>
[ExcludeFromCodeCoverage]
public class StubApiHttpClient : IApiHttpClient
{
    private readonly Dictionary<string, Func<object>> _getResponseFactories = [];
    private readonly Dictionary<string, Func<object, object>> _postResponseFactories = [];
    private readonly Dictionary<string, Func<object, object>> _putResponseFactories = [];
    private readonly HashSet<string> _deleteSuccessEndpoints = [];

    /// <summary>
    /// Gets the authorization response returned for the <c>auth/get-authorization</c> endpoint.
    /// </summary>
    public GetAuthorizationResponse AuthorizationResponse { get; set; } = new(Guid.NewGuid(), "Admin", Enum.GetValues<AuthorizationPermission>());

    /// <summary>
    /// Gets or sets the initialization response returned for the <c>initialization</c> endpoint.
    /// </summary>
    public InitializationResponse InitializationResponse { get; set; } = new() { IsInitialized = true };

    /// <summary>
    /// Gets or sets the login response returned for the <c>auth/login</c> endpoint.
    /// </summary>
    public LoginResponse LoginResponse { get; set; } = new(Guid.NewGuid(), "testuser", "test_jwt_token", false);

    /// <summary>
    /// Gets the list of GET endpoints that were requested.
    /// </summary>
    public List<string> GetEndpointsCalled { get; } = [];

    /// <summary>
    /// Gets the list of POST requests that were sent, with their payloads.
    /// </summary>
    public List<(string Endpoint, object? Data)> PostRequests { get; } = [];

    /// <summary>
    /// Gets the list of PUT requests that were sent, with their payloads.
    /// </summary>
    public List<(string Endpoint, object? Data)> PutRequests { get; } = [];

    /// <summary>
    /// Gets the list of DELETE endpoints that were requested.
    /// </summary>
    public List<string> DeleteEndpointsCalled { get; } = [];

    /// <summary>
    /// Registers the response returned when the specified GET endpoint is called.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint for which to register the response.</param>
    /// <param name="response">The response to return.</param>
    public void RegisterGetResponse<TResponse>(string endpoint, TResponse response) where TResponse : class
    {
        _getResponseFactories[endpoint] = () => response;
    }

    /// <summary>
    /// Registers the response factory invoked when the specified GET endpoint is called.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register the response factory.</param>
    /// <param name="responseFactory">The factory that produces the response.</param>
    public void RegisterGetResponseFactory(string endpoint, Func<object> responseFactory)
    {
        _getResponseFactories[endpoint] = responseFactory;
    }

    /// <summary>
    /// Registers the response returned when the specified POST endpoint is called.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint for which to register the response.</param>
    /// <param name="response">The response to return.</param>
    public void RegisterPostResponse<TResponse>(string endpoint, TResponse response) where TResponse : class
    {
        _postResponseFactories[endpoint] = _ => response;
    }

    /// <summary>
    /// Registers the response factory invoked when the specified POST endpoint is called.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register the response factory.</param>
    /// <param name="responseFactory">The factory that produces the response from the request payload.</param>
    public void RegisterPostResponseFactory(string endpoint, Func<object, object> responseFactory)
    {
        _postResponseFactories[endpoint] = responseFactory;
    }

    /// <summary>
    /// Registers the exception thrown when the specified POST endpoint is called.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register the exception.</param>
    /// <param name="exception">The exception to throw.</param>
    public void RegisterPostException(string endpoint, ApiException exception)
    {
        _postResponseFactories[endpoint] = _ => throw exception;
    }

    /// <summary>
    /// Registers the exception thrown when the specified GET endpoint is called.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register the exception.</param>
    /// <param name="exception">The exception to throw.</param>
    public void RegisterGetException(string endpoint, ApiException exception)
    {
        _getResponseFactories[endpoint] = () => throw exception;
    }

    /// <summary>
    /// Registers the response returned when the specified PUT endpoint is called.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint for which to register the response.</param>
    /// <param name="response">The response to return.</param>
    public void RegisterPutResponse<TResponse>(string endpoint, TResponse response) where TResponse : class
    {
        _putResponseFactories[endpoint] = _ => response;
    }

    /// <summary>
    /// Registers the specified DELETE endpoint as succeeding.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register success.</param>
    public void RegisterDeleteSuccess(string endpoint)
    {
        _deleteSuccessEndpoints.Add(endpoint);
    }

    /// <summary>
    /// Resets all registered responses and captured requests, keeping only the default responses.
    /// </summary>
    public void Reset()
    {
        _getResponseFactories.Clear();
        _postResponseFactories.Clear();
        _putResponseFactories.Clear();
        _deleteSuccessEndpoints.Clear();
        GetEndpointsCalled.Clear();
        PostRequests.Clear();
        PutRequests.Clear();
        DeleteEndpointsCalled.Clear();
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the GET request.</returns>
    public Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        GetEndpointsCalled.Add(endpoint);
        if (_getResponseFactories.TryGetValue(endpoint, out Func<object>? responseFactory))
            return Task.FromResult((TResponse)responseFactory());
        if (endpoint.StartsWith(ApiRoutes.Initialization.CHECK_INITIALIZATION, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult((TResponse)(object)InitializationResponse);
        if (endpoint.Contains("auth/get-authorization", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult((TResponse)(object)AuthorizationResponse);
        throw new InvalidOperationException($"No GET response is registered for the endpoint '{endpoint}'.");
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
        await Task.CompletedTask;
        yield break;
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A model containing the deserialized blob.</returns>
    public Task<BlobDataDto> GetBlobAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Blob retrieval is not supported by the test stub.");
    }

    /// <summary>
    /// Sends a DELETE request to the specified <paramref name="endpoint"/> as an asynchronous operation.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        DeleteEndpointsCalled.Add(endpoint);
        if (_deleteSuccessEndpoints.Contains(endpoint))
            return Task.CompletedTask;
        throw new ApiException(null, System.Net.HttpStatusCode.InternalServerError, endpoint);
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
    public Task<TResponse> PutAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        PutRequests.Add((endpoint, data));
        if (_putResponseFactories.TryGetValue(endpoint, out Func<object, object>? responseFactory))
            return Task.FromResult((TResponse)responseFactory(data!));
        throw new InvalidOperationException($"No PUT response is registered for the endpoint '{endpoint}'.");
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
    public Task<TResponse> PostMultipartAsync<TResponse>(string endpoint, Stream fileStream, string fileName, string fieldName, CancellationToken cancellationToken = default)
    {
        PostRequests.Add((endpoint, fileName));
        if (_postResponseFactories.TryGetValue(endpoint, out Func<object, object>? responseFactory))
            return Task.FromResult((TResponse)responseFactory(fileName));
        throw new InvalidOperationException($"No POST response is registered for the endpoint '{endpoint}'.");
    }

    /// <summary>
    /// Sends a POST request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response content.</typeparam>
    /// <typeparam name="TModel">The expected type of the payload content.</typeparam>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="data">The data to be serialized and send to the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The deserialized response containing the result of the POST request.</returns>
    public Task<TResponse> PostAsync<TResponse, TModel>(string endpoint, TModel data, CancellationToken cancellationToken = default)
    {
        PostRequests.Add((endpoint, data));
        if (_postResponseFactories.TryGetValue(endpoint, out Func<object, object>? responseFactory))
            return Task.FromResult((TResponse)responseFactory(data!));
        if (endpoint == ApiRoutes.Authentication.LOGIN_ACCOUNT)
            return Task.FromResult((TResponse)(object)LoginResponse);
        throw new InvalidOperationException($"No POST response is registered for the endpoint '{endpoint}'.");
    }
}
