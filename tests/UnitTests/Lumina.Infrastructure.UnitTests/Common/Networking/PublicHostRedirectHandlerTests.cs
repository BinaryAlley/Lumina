#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Networking;

/// <summary>
/// Contains unit tests for the <see cref="PublicHostRedirectHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PublicHostRedirectHandlerTests
{
    [Fact]
    public async Task SendAsync_WhenTheInitialRequestHostIsPublic_ShouldSendTheRequest()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient client = CreateClient(innerHandler);

        // Act
        HttpResponseMessage response = await client.GetAsync("https://example.com/cover.jpg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Theory]
    [InlineData("http://127.0.0.1/cover.jpg")] // the IPv4 loopback address
    [InlineData("http://localhost/cover.jpg")] // the loopback host name
    [InlineData("http://[::1]/cover.jpg")] // the IPv6 loopback address
    [InlineData("http://169.254.169.254/latest/meta-data/")] // the link local cloud metadata endpoint
    [InlineData("http://10.0.0.5/cover.jpg")] // a private class A address
    [InlineData("http://172.16.0.1/cover.jpg")] // a private class B address
    [InlineData("http://192.168.1.10/cover.jpg")] // a private class C address
    public async Task SendAsync_WhenTheInitialRequestHostIsPrivateOrLocal_ShouldThrowAndNotSendTheRequest(string url)
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient client = CreateClient(innerHandler);

        // Act
        Task Act()
        {
            return client.GetAsync(url);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(0, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheInitialRequestRedirectsToAPublicHttpsHost_ShouldFollowIt()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            if (callIndex == 1)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri("https://archive.org/download/mbid-1/index.json");
                return redirect;
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using HttpClient client = CreateClient(innerHandler);

        // Act
        HttpResponseMessage response = await client.GetAsync("https://coverartarchive.org/release/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, innerHandler.CallCount);
        Assert.Equal("archive.org", innerHandler.Requests[1].RequestUri!.Host);
    }

    [Theory]
    [InlineData("http://archive.org/index.json")] // a downgrade of a trusted host to plain HTTP
    [InlineData("https://127.0.0.1/index.json")] // the IPv4 loopback address
    [InlineData("https://localhost/index.json")] // the loopback host name
    [InlineData("https://169.254.169.254/latest/meta-data/")] // the link local cloud metadata endpoint
    [InlineData("https://10.0.0.5/index.json")] // a private class A address
    [InlineData("https://192.168.1.10/index.json")] // a private class C address
    public async Task SendAsync_WhenTheInitialRequestRedirectsToADisallowedTarget_ShouldThrow(string location)
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri(location);
            return redirect;
        });
        using HttpClient client = CreateClient(innerHandler);

        // Act
        Task Act()
        {
            return client.GetAsync("https://coverartarchive.org/release/1");
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheRequestIsRedirectedTooManyTimes_ShouldThrow()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri("https://archive.org/loop/index.json");
            return redirect;
        });
        using HttpClient client = CreateClient(innerHandler);

        // Act
        Task Act()
        {
            return client.GetAsync("https://coverartarchive.org/release/1");
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        // The initial request plus the maximum number of followed redirects are sent before the loop is detected.
        Assert.Equal(6, innerHandler.CallCount);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> whose requests are sent through the handler under test, wrapping the provided inner handler.
    /// </summary>
    /// <param name="innerHandler">The handler that answers the requests that reach the network.</param>
    /// <returns>The created client.</returns>
    private static HttpClient CreateClient(HttpMessageHandler innerHandler)
    {
        PublicHostRedirectHandler handler = new() { InnerHandler = innerHandler };
        return new HttpClient(handler);
    }

    /// <summary>
    /// Test HTTP message handler that answers every request with a canned response produced by a factory, and records the requests that were sent.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class StubHttpMessageHandler : HttpMessageHandler
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
            return Task.FromResult(_responseFactory(request, callIndex));
        }
    }
}
