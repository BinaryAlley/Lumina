#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.Networking;
using Nemera.Infrastructure.Common.Networking;
using Nemera.Plugins.Contracts.Common.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.UnitTests.Common.Networking;

/// <summary>
/// Contains unit tests for the <see cref="PublicHostRedirectHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PublicHostRedirectHandlerTests
{
    private const string PRIVATE_HOST = "internal.example.com";

    private static readonly IPAddress s_publicAddress = IPAddress.Parse("93.184.216.34");

    [Fact]
    public async Task SendAsync_WhenTheResponseRedirectsToAPublicHttpsHost_ShouldFollowIt()
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

    [Fact]
    public async Task SendAsync_WhenTheResponseRedirectsToAPrivateHostName_ShouldThrow()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri($"https://{PRIVATE_HOST}/index.json");
            return redirect;
        });
        using HttpClient client = CreateClient(innerHandler);

        // Act
        Task Act()
        {
            return client.GetAsync("https://coverartarchive.org/release/1");
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Theory]
    [InlineData("http://archive.org/index.json")] // A downgrade of a trusted host to plain HTTP.
    [InlineData("https://127.0.0.1/index.json")] // The IPv4 loopback address.
    [InlineData("https://localhost/index.json")] // The loopback host name.
    [InlineData("https://169.254.169.254/latest/meta-data/")] // The link local cloud metadata endpoint.
    [InlineData("https://10.0.0.5/index.json")] // A private class A address.
    [InlineData("https://192.168.1.10/index.json")] // A private class C address.
    public async Task SendAsync_WhenTheResponseRedirectsToADisallowedTarget_ShouldThrow(string location)
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
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheRequestAllowsPrivateEndPointsAndRedirectsToAPrivateHost_ShouldFollowIt()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            if (callIndex == 1)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri($"https://{PRIVATE_HOST}/index.json");
                return redirect;
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, $"https://{PRIVATE_HOST}/start");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, PRIVATE_HOST);

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, innerHandler.CallCount);
        Assert.Equal(PRIVATE_HOST, innerHandler.Requests[1].RequestUri!.Host);
    }

    [Fact]
    public async Task SendAsync_WhenARequestWithABodyIsRedirected_ShouldReturnTheRedirectResponseWithoutFollowingIt()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri("https://archive.org/index.json");
            return redirect;
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Post, "https://coverartarchive.org/release/1");

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
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
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        // The initial request plus the maximum number of followed redirects are sent before the loop is detected.
        Assert.Equal(6, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheRequestDisablesRedirectFollowing_ShouldReturnTheRedirectResponseWithoutFollowingIt()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri("https://archive.org/index.json");
            return redirect;
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, "https://coverartarchive.org/release/1");
        request.Options.Set(OutboundRequestOptions.FollowRedirects, false);

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://archive.org/index.json", response.Headers.Location!.AbsoluteUri);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheRedirectPointsAtAnotherPrivateHostThanTheAllowedOne_ShouldThrow()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri("https://another.internal.example.com/index.json");
            return redirect;
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, $"https://{PRIVATE_HOST}/start");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, PRIVATE_HOST);

        // Act
        Task Act()
        {
            return client.SendAsync(request);
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenARedirectCrossesOrigins_ShouldNotForwardCredentialsOrHopByHopHeaders()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            if (callIndex == 1)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri("https://archive.org/index.json");
                return redirect;
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, "https://coverartarchive.org/start");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
        request.Headers.TryAddWithoutValidation("Cookie", "session=secret");
        request.Headers.TryAddWithoutValidation("Proxy-Authorization", "Basic secret");
        request.Headers.Host = "coverartarchive.org";
        request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
        request.Headers.TryAddWithoutValidation("Keep-Alive", "timeout=5");
        request.Headers.TryAddWithoutValidation("TE", "trailers");
        request.Headers.TryAddWithoutValidation("Upgrade", "h2c");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        HttpRequestMessage redirectedRequest = innerHandler.Requests[1];
        Assert.Null(redirectedRequest.Headers.Authorization);
        Assert.False(redirectedRequest.Headers.Contains("Cookie"));
        Assert.False(redirectedRequest.Headers.Contains("Proxy-Authorization"));
        Assert.Null(redirectedRequest.Headers.Host);
        Assert.False(redirectedRequest.Headers.Contains("Connection"));
        Assert.False(redirectedRequest.Headers.Contains("Keep-Alive"));
        Assert.False(redirectedRequest.Headers.Contains("TE"));
        Assert.False(redirectedRequest.Headers.Contains("Upgrade"));
        Assert.Contains("application/json", redirectedRequest.Headers.Accept.ToString());
    }

    [Fact]
    public async Task SendAsync_WhenARedirectStaysOnTheSameOrigin_ShouldKeepCredentialsButDropHopByHopHeaders()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            if (callIndex == 1)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri("https://coverartarchive.org/index.json");
                return redirect;
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, "https://coverartarchive.org/start");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
        request.Headers.TryAddWithoutValidation("Cookie", "session=secret");
        request.Headers.TryAddWithoutValidation("Connection", "keep-alive");

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        HttpRequestMessage redirectedRequest = innerHandler.Requests[1];
        Assert.Equal("Bearer secret", redirectedRequest.Headers.Authorization!.ToString());
        string cookieHeader = Assert.Single(redirectedRequest.Headers.GetValues("Cookie"));
        Assert.Equal("session=secret", cookieHeader);
        Assert.False(redirectedRequest.Headers.Contains("Connection"));
    }

    [Fact]
    public async Task SendAsync_WhenAHeadRequestIsRedirected_ShouldFollowIt()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            if (callIndex == 1)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri("https://archive.org/index.json");
                return redirect;
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Head, "https://coverartarchive.org/release/1");

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenAnAllowedPrivateHostRedirectsToItselfAcrossMultipleHops_ShouldKeepThePermission()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            if (callIndex <= 2)
            {
                HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
                redirect.Headers.Location = new Uri(callIndex == 1 ? $"https://{PRIVATE_HOST}/second" : $"https://{PRIVATE_HOST}/third");
                return redirect;
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, $"https://{PRIVATE_HOST}/start");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, PRIVATE_HOST);

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheResponseRedirectsToPlainHttp_ShouldThrowAPublicHostRejectedException()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri("http://archive.org/index.json");
            return redirect;
        });
        using HttpClient client = CreateClient(innerHandler);

        // Act
        Task Act()
        {
            return client.GetAsync("https://coverartarchive.org/release/1");
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.RedirectSchemeNotHttps, exception.RejectionReason);
        Assert.Equal(1, innerHandler.CallCount);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> whose requests are sent through the handler under test, wrapping the provided inner handler.
    /// </summary>
    /// <param name="innerHandler">The handler that answers the requests that reach the network.</param>
    /// <returns>The created client.</returns>
    private static HttpClient CreateClient(HttpMessageHandler innerHandler)
    {
        PublicHostValidator publicHostValidator = new((host, cancellationToken) =>
            Task.FromResult<IPAddress[]>(host.Contains("internal.example.com", StringComparison.OrdinalIgnoreCase) ? [IPAddress.Parse("10.0.0.5")] : [s_publicAddress]));
        PublicHostRedirectHandler handler = new(publicHostValidator, NullLogger<PublicHostRedirectHandler>.Instance) { InnerHandler = innerHandler };
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
