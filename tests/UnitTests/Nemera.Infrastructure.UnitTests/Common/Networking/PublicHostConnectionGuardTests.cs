#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.Networking;
using Nemera.Infrastructure.Common.Networking;
using Nemera.Plugins.Contracts.Common.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.UnitTests.Common.Networking;

/// <summary>
/// Contains unit tests for the <see cref="PublicHostConnectionGuard"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PublicHostConnectionGuardTests
{
    private static readonly IPAddress s_publicAddress = IPAddress.Parse("93.184.216.34");

    [Fact]
    public void CreatePrimaryHandler_WhenCalled_ShouldDisableAutomaticRedirectsAndInstallTheConnectGuard()
    {
        // Arrange
        PublicHostValidator publicHostValidator = CreateValidator(host => [s_publicAddress]);

        // Act
        using SocketsHttpHandler handler = PublicHostConnectionGuard.CreatePrimaryHandler(publicHostValidator, NullLogger.Instance);

        // Assert
        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseProxy);
        Assert.NotNull(handler.ConnectCallback);
        // The callback behavior itself is covered by CreatePrimaryHandler_WhenARequestConnects_ShouldUseTheAllowedPrivateHostOfTheInitialRequest.
    }

    [Fact]
    public async Task ConnectAsync_WhenTheHostResolvesToOnePrivateAddress_ShouldThrowWithoutConnecting()
    {
        // Arrange
        PublicHostValidator publicHostValidator = CreateValidator(host => [s_publicAddress, IPAddress.Parse("10.0.0.5")]);

        // Act
        Task Act()
        {
            return PublicHostConnectionGuard.ConnectAsync(publicHostValidator, "mixed.example.com", 443, request: null, NullLogger.Instance, CancellationToken.None).AsTask();
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.AddressNotAllowed, exception.RejectionReason);
    }

    [Fact]
    public async Task ConnectAsync_WhenTheRequestUriHasAUnicodeIdnHostAndTheConnectionUsesThePunycodeHost_ShouldValidateTheResolvedAddress()
    {
        // Arrange
        // Uri.Host keeps the Unicode spelling of an internationalized name, while the connection authority uses its punycode spelling. The two must never be
        // compared to decide whether a connection goes to a proxy, because an attacker controlled name could otherwise skip the address check.
        PublicHostValidator publicHostValidator = CreateValidator(host => [IPAddress.Loopback]);
        using HttpRequestMessage request = new(HttpMethod.Get, "https://bücher.example/cover.jpg");

        // Act
        Task Act()
        {
            return PublicHostConnectionGuard.ConnectAsync(publicHostValidator, "xn--bcher-kva.example", 1, request, NullLogger.Instance, CancellationToken.None).AsTask();
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.AddressNotAllowed, exception.RejectionReason);
        Assert.DoesNotContain("127.0.0.1", exception.Message);
    }

    [Fact]
    public async Task ConnectAsync_WhenAnotherHostResolvesToAPrivateAddress_ShouldThrowWithoutConnecting()
    {
        // Arrange
        PublicHostValidator publicHostValidator = CreateValidator(host => [IPAddress.Loopback]);
        using HttpRequestMessage request = new(HttpMethod.Get, "http://other.internal.example.com/cover.jpg");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, "internal.example.com");

        // Act
        Task Act()
        {
            return PublicHostConnectionGuard.ConnectAsync(publicHostValidator, "other.internal.example.com", 443, request, NullLogger.Instance, CancellationToken.None).AsTask();
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.AddressNotAllowed, exception.RejectionReason);
    }

    [Fact]
    public async Task ConnectAsync_WhenTheAllowedPrivateHostResolvesToTheLinkLocalMetadataAddress_ShouldThrowWithoutConnecting()
    {
        // Arrange
        PublicHostValidator publicHostValidator = CreateValidator(host => [IPAddress.Parse("169.254.169.254")]);
        using HttpRequestMessage request = new(HttpMethod.Get, "http://internal.example.com/cover.jpg");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, "internal.example.com");

        // Act
        Task Act()
        {
            return PublicHostConnectionGuard.ConnectAsync(publicHostValidator, "internal.example.com", 443, request, NullLogger.Instance, CancellationToken.None).AsTask();
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.LinkLocalOrMetadataAddress, exception.RejectionReason);
    }

    [Fact]
    public async Task ConnectAsync_WhenTheFirstAddressIsUnreachable_ShouldConnectToTheNextAddress()
    {
        // Arrange
        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        // 10.0.0.1 is a private address that no test service listens on, so the loopback address wins the staggered race.
        PublicHostValidator publicHostValidator = CreateValidator(host => [IPAddress.Parse("10.0.0.1"), IPAddress.Loopback]);
        using HttpRequestMessage request = new(HttpMethod.Get, "http://mixed.example.com/cover.jpg");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, "mixed.example.com");
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(10));

        // Act
        Task<Stream> connectTask = PublicHostConnectionGuard.ConnectAsync(publicHostValidator, "mixed.example.com", port, request, NullLogger.Instance, CancellationToken.None).AsTask();
        (TcpClient acceptedClient, Stream stream) = await AcceptConnectionAsync(listener, connectTask, cancellationTokenSource);
        using (acceptedClient)
        using (stream)
        {
            // Assert
            Assert.True(stream.CanWrite);
        }
    }

    [Fact]
    public async Task CreatePrimaryHandler_WhenTheConnectPolicyRejectsTheAddress_ShouldWrapThePolicyRejectionAsAConnectionError()
    {
        // Arrange
        PublicHostValidator publicHostValidator = CreateValidator(host => [IPAddress.Loopback]);
        using SocketsHttpHandler handler = PublicHostConnectionGuard.CreatePrimaryHandler(publicHostValidator, NullLogger.Instance);
        using HttpClient client = new(handler);
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(10));

        // Act
        Task Act()
        {
            return client.GetAsync("http://internal.example.com/", cancellationTokenSource.Token);
        }

        // Assert
        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(Act);
        Assert.Equal(HttpRequestError.ConnectionError, exception.HttpRequestError);
        PublicHostRejectedException innerException = Assert.IsType<PublicHostRejectedException>(exception.InnerException);
        Assert.Equal(HttpRequestError.ConfigurationLimitExceeded, innerException.HttpRequestError);
        Assert.Equal(OutboundRequestRejectionReason.AddressNotAllowed, innerException.RejectionReason);
    }

    [Fact]
    public async Task CreatePrimaryHandler_WhenARequestConnects_ShouldUseTheAllowedPrivateHostOfTheInitialRequest()
    {
        // Arrange
        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        PublicHostValidator publicHostValidator = CreateValidator(host => [IPAddress.Loopback]);
        using SocketsHttpHandler handler = PublicHostConnectionGuard.CreatePrimaryHandler(publicHostValidator, NullLogger.Instance);
        using HttpClient client = new(handler);
        using HttpRequestMessage request = new(HttpMethod.Get, $"http://internal.example.com:{port}/");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, "internal.example.com");
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(10));

        // Act
        Task<HttpResponseMessage> requestTask = client.SendAsync(request, cancellationTokenSource.Token);
        Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync(cancellationTokenSource.Token).AsTask();
        try
        {
            Task firstCompletedTask = await Task.WhenAny(requestTask, acceptTask);
            // Await the request first when it completed, so a failure to connect surfaces its own exception instead of the accept timeout.
            if (firstCompletedTask == requestTask)
                await requestTask;

            using TcpClient acceptedClient = await acceptTask;
            using NetworkStream stream = acceptedClient.GetStream();
            byte[] responseBytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(responseBytes, cancellationTokenSource.Token);
            using HttpResponseMessage response = await requestTask;

            // Assert
            // The connect callback resolved the host through the validator and read the allowed private host from the initial request message, because
            // without that option the loopback address would have been rejected and the connection would never have been accepted.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            cancellationTokenSource.Cancel();
        }
    }

    [Fact]
    public async Task ConnectAsync_WhenTheRequestAllowsPrivateEndPoints_ShouldConnectToThePrivateAddress()
    {
        // Arrange
        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        PublicHostValidator publicHostValidator = CreateValidator(host => [IPAddress.Loopback]);
        using HttpRequestMessage request = new(HttpMethod.Get, "http://internal.example.com/cover.jpg");
        request.Options.Set(OutboundRequestOptions.AllowedPrivateHost, "internal.example.com");
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(10));

        // Act
        Task<Stream> connectTask = PublicHostConnectionGuard.ConnectAsync(publicHostValidator, "internal.example.com", port, request, NullLogger.Instance, CancellationToken.None).AsTask();
        (TcpClient acceptedClient, Stream stream) = await AcceptConnectionAsync(listener, connectTask, cancellationTokenSource);
        using (acceptedClient)
        using (stream)
        {
            // Assert
            Assert.True(stream.CanWrite);
        }
    }

    /// <summary>
    /// Accepts the connection made by <paramref name="connectTask"/>, surfacing a connection failure instead of waiting for the accept timeout.
    /// </summary>
    /// <param name="listener">The listener that accepts the connection.</param>
    /// <param name="connectTask">The task that establishes the connection.</param>
    /// <param name="cancellationTokenSource">The cancellation token source that bounds the accept.</param>
    /// <returns>The accepted client and the established stream.</returns>
    private static async Task<(TcpClient AcceptedClient, Stream Stream)> AcceptConnectionAsync(TcpListener listener, Task<Stream> connectTask, CancellationTokenSource cancellationTokenSource)
    {
        Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync(cancellationTokenSource.Token).AsTask();
        try
        {
            Task firstCompletedTask = await Task.WhenAny(connectTask, acceptTask).ConfigureAwait(false);
            // Await the connect task first when it completed, so a failure to connect surfaces its own exception instead of the accept timeout.
            if (firstCompletedTask == connectTask)
                await connectTask.ConfigureAwait(false);

            TcpClient acceptedClient = await acceptTask.ConfigureAwait(false);
            Stream stream = await connectTask.ConfigureAwait(false);
            return (acceptedClient, stream);
        }
        catch
        {
            cancellationTokenSource.Cancel();
            try
            {
                await acceptTask.ConfigureAwait(false);
            }
            catch
            {
                // The accept was canceled because the connection failed; its exception must not mask the connection failure.
            }
            throw;
        }
    }

    /// <summary>
    /// Creates a <see cref="PublicHostValidator"/> whose host name resolution is replaced by the provided resolver.
    /// </summary>
    /// <param name="addressResolver">The resolver that produces the addresses of a host name.</param>
    /// <returns>The created validator.</returns>
    private static PublicHostValidator CreateValidator(Func<string, IPAddress[]> addressResolver)
    {
        return new PublicHostValidator((host, cancellationToken) => Task.FromResult(addressResolver(host)));
    }
}
