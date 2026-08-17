#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Api;

/// <summary>
/// <see cref="HttpMessageHandler"/> test double that returns a configurable response for every request sent through it.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestApiHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

    /// <summary>
    /// Gets the list of requests that were sent through this handler.
    /// </summary>
    public List<HttpRequestMessage> Requests { get; } = [];

    /// <summary>
    /// Gets the bodies of the requests that were sent through this handler, captured while the request content is still available.
    /// </summary>
    public List<string?> RequestBodies { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TestApiHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="responseFactory">The factory that produces the response for a given request.</param>
    public TestApiHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    /// <summary>
    /// Sends the request to the configured response factory.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The configured HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        RequestBodies.Add(request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken));
        return await Task.FromResult(_responseFactory(request));
    }
}
