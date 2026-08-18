#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Setup;

/// <summary>
/// <see cref="HttpMessageHandler"/> test double that routes requests to the configured responses based on their absolute path.
/// </summary>
[ExcludeFromCodeCoverage]
internal class StubOpenLibraryHttpMessageHandler : HttpMessageHandler
{
    private readonly List<(Func<HttpRequestMessage, bool> Matches, Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory)> _routes = [];

    /// <summary>
    /// Gets the list of requests that were sent through this handler.
    /// </summary>
    public List<HttpRequestMessage> Requests { get; } = [];

    /// <summary>
    /// Routes requests with the given absolute path to the provided JSON response.
    /// </summary>
    /// <param name="absolutePath">The absolute path of the request URL to match.</param>
    /// <param name="json">The JSON payload of the response.</param>
    /// <returns>This handler, for chaining.</returns>
    public StubOpenLibraryHttpMessageHandler MapPath(string absolutePath, string json)
    {
        return AddRoute(
            request => string.Equals(request.RequestUri?.AbsolutePath, absolutePath, StringComparison.Ordinal),
            _ => CreateJsonResponse(HttpStatusCode.OK, json));
    }

    /// <summary>
    /// Routes requests matching the given predicate to the response produced by the provided factory.
    /// </summary>
    /// <param name="matches">The predicate that determines whether the request matches the route.</param>
    /// <param name="responseFactory">The factory that produces the response for a matching request.</param>
    /// <returns>This handler, for chaining.</returns>
    public StubOpenLibraryHttpMessageHandler AddRoute(Func<HttpRequestMessage, bool> matches, Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _routes.Add((matches, responseFactory));
        return this;
    }

    /// <summary>
    /// Creates a JSON response with the given status code and payload.
    /// </summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="json">The JSON payload of the response.</param>
    /// <returns>The created JSON response.</returns>
    public static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// Sends the request to the first matching route, or returns a not found response.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The HTTP response message for the request.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        foreach ((Func<HttpRequestMessage, bool> matches, Func<HttpRequestMessage, HttpResponseMessage> responseFactory) in _routes)
            if (matches(request))
                return Task.FromResult(responseFactory(request));

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
