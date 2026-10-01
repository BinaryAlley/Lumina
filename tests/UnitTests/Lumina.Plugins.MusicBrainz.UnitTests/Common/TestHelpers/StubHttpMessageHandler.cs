#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.TestHelpers;

/// <summary>
/// Test HTTP message handler that answers every request with a canned response produced by a factory, and records the requests that were sent.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, int, HttpResponseMessage> _responseFactory;
    private int _callCount;

    /// <summary>
    /// Gets the requests that were sent through the handler.
    /// </summary>
    public List<HttpRequestMessage> Requests { get; } = [];

    /// <summary>
    /// Gets the number of requests that were sent through the handler.
    /// </summary>
    public int CallCount => _callCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="StubHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="responseFactory">The factory that produces the response of a request, given the request and its one based call index.</param>
    public StubHttpMessageHandler(Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    /// <summary>
    /// Sends the provided request, producing its response through the configured factory.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The response produced for the request.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        int callIndex = Interlocked.Increment(ref _callCount);
        Requests.Add(request);
        try
        {
            return Task.FromResult(_responseFactory(request, callIndex));
        }
        catch (Exception exception)
        {
            // A response factory may throw to simulate a transport failure, which is surfaced as a faulted task.
            return Task.FromException<HttpResponseMessage>(exception);
        }
    }
}
