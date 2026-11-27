#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Api;

/// <summary>
/// Test double for the <see cref="IStreamingApiHttpClient"/> interface, allowing integration and security tests to stub the
/// streamed responses of the remote API without hosting a real backend.
/// </summary>
[ExcludeFromCodeCoverage]
public class StubStreamingApiHttpClient : IStreamingApiHttpClient
{
    private readonly Dictionary<string, Func<ApiStreamResult>> _streamResponseFactories = [];
    private readonly Dictionary<string, Func<Exception>> _streamExceptionFactories = [];

    /// <summary>
    /// Gets the streamed requests that were sent, with the headers that were forwarded with them.
    /// </summary>
    public List<StubStreamRequest> StreamRequests { get; } = [];

    /// <summary>
    /// Gets the list of endpoints the streams were requested from.
    /// </summary>
    public List<string> GetEndpointsCalled { get; } = [];

    /// <summary>
    /// Registers the streamed response returned when the specified endpoint is called.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register the response.</param>
    /// <param name="response">The streamed response to return.</param>
    public void RegisterStreamResponse(string endpoint, ApiStreamResult response)
    {
        _streamResponseFactories[endpoint] = () => response;
    }

    /// <summary>
    /// Registers the response factory invoked when the specified endpoint is called.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register the response factory.</param>
    /// <param name="responseFactory">The factory that produces the streamed response.</param>
    public void RegisterStreamResponseFactory(string endpoint, Func<ApiStreamResult> responseFactory)
    {
        _streamResponseFactories[endpoint] = responseFactory;
    }

    /// <summary>
    /// Registers the exception thrown when the specified endpoint is called.
    /// </summary>
    /// <param name="endpoint">The API endpoint for which to register the exception.</param>
    /// <param name="exception">The exception to throw.</param>
    public void RegisterStreamException(string endpoint, Exception exception)
    {
        _streamExceptionFactories[endpoint] = () => exception;
    }

    /// <summary>
    /// Resets all registered responses and captured requests.
    /// </summary>
    public void Reset()
    {
        _streamResponseFactories.Clear();
        _streamExceptionFactories.Clear();
        StreamRequests.Clear();
        GetEndpointsCalled.Clear();
    }

    /// <summary>
    /// Sends a GET request to the specified <paramref name="endpoint"/> as an asynchronous operation and returns the response as a stream.
    /// </summary>
    /// <param name="endpoint">The API endpoint where the request is being sent.</param>
    /// <param name="requestHeaders">The range and conditional headers of the incoming request to forward.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The streamed response of the API request.</returns>
    public Task<ApiStreamResult> GetStreamAsync(string endpoint, IReadOnlyDictionary<string, string> requestHeaders, CancellationToken cancellationToken = default)
    {
        GetEndpointsCalled.Add(endpoint);
        StreamRequests.Add(new StubStreamRequest(endpoint, new Dictionary<string, string>(requestHeaders)));
        if (_streamExceptionFactories.TryGetValue(endpoint, out Func<Exception>? exceptionFactory))
            throw exceptionFactory();
        if (_streamResponseFactories.TryGetValue(endpoint, out Func<ApiStreamResult>? responseFactory))
            return Task.FromResult(responseFactory());
        throw new InvalidOperationException($"No stream response is registered for the endpoint '{endpoint}'.");
    }
}
