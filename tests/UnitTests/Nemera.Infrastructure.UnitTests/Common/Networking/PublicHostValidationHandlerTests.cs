#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.Networking;
using Nemera.Infrastructure.Common.Networking;
using Nemera.Plugins.Contracts.Common.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.UnitTests.Common.Networking;

/// <summary>
/// Contains unit tests for the <see cref="PublicHostValidationHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PublicHostValidationHandlerTests
{
    private const string PRIVATE_HOST = "internal.example.com";

    private static readonly IPAddress s_publicAddress = IPAddress.Parse("93.184.216.34");

    [Fact]
    public async Task SendAsync_WhenTheHostIsPublic_ShouldSendTheRequest()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new();
        using HttpClient client = CreateClient(innerHandler);

        // Act
        using HttpResponseMessage response = await client.GetAsync("https://example.com/cover.jpg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Theory]
    [InlineData("http://127.0.0.1/cover.jpg")] // The IPv4 loopback address.
    [InlineData("http://localhost/cover.jpg")] // The loopback host name.
    [InlineData("http://[::1]/cover.jpg")] // The IPv6 loopback address.
    [InlineData("http://169.254.169.254/latest/meta-data/")] // The link local cloud metadata endpoint.
    [InlineData("http://10.0.0.5/cover.jpg")] // A private class A address.
    [InlineData("http://172.16.0.1/cover.jpg")] // A private class B address.
    [InlineData("http://192.168.1.10/cover.jpg")] // A private class C address.
    public async Task SendAsync_WhenTheHostIsPrivateOrLocal_ShouldThrowAndNotSendTheRequest(string url)
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new();
        using HttpClient client = CreateClient(innerHandler);

        // Act
        Task Act()
        {
            return client.GetAsync(url);
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(0, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheHostNameResolvesToOnePrivateAddress_ShouldThrowAndNotSendTheRequest()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new();
        using HttpClient client = CreateClient(innerHandler);

        // Act
        Task Act()
        {
            return client.GetAsync($"https://{PRIVATE_HOST}/cover.jpg");
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.AddressNotAllowed, exception.RejectionReason);
        Assert.Contains(PRIVATE_HOST, exception.Message);
        Assert.DoesNotContain("10.0.0.5", exception.Message);
        Assert.Equal(0, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheHostCannotBeResolved_ShouldThrowAPublicHostResolutionException()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new();
        PublicHostValidator validator = new((host, cancellationToken) => throw new SocketException((int)SocketError.HostNotFound));
        PublicHostValidationHandler handler = new(validator, NullLogger<PublicHostValidationHandler>.Instance) { InnerHandler = innerHandler };
        using HttpClient client = new(handler);

        // Act
        Task Act()
        {
            return client.GetAsync("https://missing.example.com/cover.jpg");
        }

        // Assert
        PublicHostResolutionException exception = await Assert.ThrowsAsync<PublicHostResolutionException>(Act);
        Assert.Equal(HttpRequestError.NameResolutionError, exception.HttpRequestError);
        Assert.Equal(0, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheHostIsRejected_ShouldLogAWarning()
    {
        // Arrange
        ILogger<PublicHostValidationHandler> mockLogger = Substitute.For<ILogger<PublicHostValidationHandler>>();
        mockLogger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        StubHttpMessageHandler innerHandler = new();
        PublicHostValidationHandler handler = new(CreateValidator(), mockLogger) { InnerHandler = innerHandler };
        using HttpClient client = new(handler);

        // Act
        Task Act()
        {
            return client.GetAsync($"https://{PRIVATE_HOST}/cover.jpg");
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendAsync_WhenTheRequestAllowsPrivateEndPointsForTheHost_ShouldSendTheRequest()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new();
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, $"https://{PRIVATE_HOST}/cover.jpg");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, PRIVATE_HOST);

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheRequestAllowsPrivateEndPointsForAnotherHost_ShouldThrowAndNotSendTheRequest()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new();
        using HttpClient client = CreateClient(innerHandler);
        using HttpRequestMessage request = new(HttpMethod.Get, $"https://{PRIVATE_HOST}/cover.jpg");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, "another.internal.example.com");

        // Act
        Task Act()
        {
            return client.SendAsync(request);
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(0, innerHandler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheRequestHasNoAbsoluteUri_ShouldThrowAndNotSendTheRequest()
    {
        // Arrange
        StubHttpMessageHandler innerHandler = new();
        PublicHostValidationHandler handler = new(CreateValidator(), NullLogger<PublicHostValidationHandler>.Instance) { InnerHandler = innerHandler };
        using HttpMessageInvoker invoker = new(handler);

        // Act
        Task Act()
        {
            return invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/relative"), CancellationToken.None);
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.RequestHasNoAbsoluteUri, exception.RejectionReason);
        Assert.Equal(0, innerHandler.CallCount);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> whose requests are sent through the handler under test, wrapping the provided inner handler.
    /// </summary>
    /// <param name="innerHandler">The handler that answers the requests that reach the network.</param>
    /// <returns>The created client.</returns>
    private static HttpClient CreateClient(HttpMessageHandler innerHandler)
    {
        PublicHostValidationHandler handler = new(CreateValidator(), NullLogger<PublicHostValidationHandler>.Instance) { InnerHandler = innerHandler };
        return new HttpClient(handler);
    }

    /// <summary>
    /// Creates a <see cref="PublicHostValidator"/> that classifies <see cref="PRIVATE_HOST"/> as private and every other host name as public.
    /// </summary>
    /// <returns>The created validator.</returns>
    private static PublicHostValidator CreateValidator()
    {
        return new PublicHostValidator((host, cancellationToken) =>
            Task.FromResult<IPAddress[]>(string.Equals(host, PRIVATE_HOST, StringComparison.OrdinalIgnoreCase) ? [IPAddress.Parse("10.0.0.5")] : [s_publicAddress]));
    }

    /// <summary>
    /// Test HTTP message handler that answers every request with a successful response and records how many requests were sent to it.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private int _callCount;

        /// <summary>
        /// Gets the number of requests that were sent through the handler.
        /// </summary>
        public int CallCount => _callCount;

        /// <summary>
        /// Sends the provided request, producing a successful response.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>The response produced for the request.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
