#region ========================================================================= USING =====================================================================================
using Nemera.Plugins.Contracts.Common.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.Common.Networking;

/// <summary>
/// Creates the primary message handler used by the outbound HTTP clients, whose connect callback resolves each host and connects only to an address that was
/// checked to be public, so that a name that changes its answer between the check and the connection cannot make the host reach an internal address.
/// </summary>
internal static class PublicHostConnectionGuard
{
    private const int ATTEMPT_STAGGER_IN_MILLISECONDS = 250;

    /// <summary>
    /// Creates the primary message handler of an outbound HTTP client, which follows no redirects and validates the address it connects to.
    /// </summary>
    /// <param name="publicHostValidator">The validator that decides whether a host is public.</param>
    /// <param name="logger">The logger that records the rejected connections.</param>
    /// <returns>The primary message handler of the client.</returns>
    /// <remarks>
    /// Automatic redirects are disabled because a redirect is a server controlled instruction, which the clients that follow one validate hop by hop instead.
    /// Proxy use is disabled as well, because a proxy would make the connection to an address other than the target of the request, which could not be pinned to
    /// the address that was checked. Every connection is therefore direct, so the connect callback is the sole and authoritative address check.
    /// </remarks>
    public static SocketsHttpHandler CreatePrimaryHandler(PublicHostValidator publicHostValidator, ILogger logger)
    {
        return new SocketsHttpHandler
        {
            // Redirects are handled by PublicHostRedirectHandler, which validates every hop. The primary handler must not follow one on its own.
            AllowAutoRedirect = false,
            // A proxy would connect to an address other than the validated target, so the guard requires direct connections.
            UseProxy = false,
            ConnectCallback = (connectionContext, cancellationToken) => ConnectAsync(
                publicHostValidator,
                connectionContext.DnsEndPoint.Host,
                connectionContext.DnsEndPoint.Port,
                connectionContext.InitialRequestMessage,
                logger,
                cancellationToken)
        };
    }

    /// <summary>
    /// Resolves the host and connects to the first of its validated addresses that accepts the connection.
    /// </summary>
    /// <param name="publicHostValidator">The validator that decides whether a host is public.</param>
    /// <param name="host">The host to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="request">The request that triggered the connection, when it is known.</param>
    /// <param name="logger">The logger that records the rejected connections.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The stream of the established connection.</returns>
    /// <exception cref="PublicHostResolutionException">The host could not be resolved.</exception>
    /// <exception cref="PublicHostRejectedException">The host resolves to an address that is not allowed, or none of its addresses accepts the connection.</exception>
    internal static async ValueTask<Stream> ConnectAsync(
        PublicHostValidator publicHostValidator,
        string host,
        int port,
        HttpRequestMessage? request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            // Resolve and validate the addresses before any socket is opened, so the connection can only target a checked endpoint.
            IReadOnlyList<IPEndPoint> endPoints = await publicHostValidator.ResolveEndPointsAsync(host, port, GetAllowedPrivateHost(request), cancellationToken).ConfigureAwait(false);
            return await ConnectToFirstEndPointAsync(endPoints, cancellationToken).ConfigureAwait(false);
        }
        catch (PublicHostRejectedException exception)
        {
            // The address policy refused the target. This is deterministic, so it is logged as a rejection rather than as a transport failure.
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Rejected the outbound connection to the host '{Host}' because {RejectionReason}. The connection was not made.", host, exception.RejectionReason);
            throw;
        }
        catch (PublicHostResolutionException)
        {
            // The host name could not be resolved. This is reported separately from a policy rejection because it can be transient.
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Could not resolve the outbound host '{Host}'. The connection was not made.", host);
            throw;
        }
    }

    /// <summary>
    /// Gets the single host the request was explicitly allowed to reach private or local addresses on, which is how a user opted into a provider that runs on the
    /// local network, like a self hosted MusicBrainz instance.
    /// </summary>
    /// <param name="request">The request that triggered the connection, when it is known.</param>
    /// <returns>The host that may be private, or <see langword="null"/> when no such permission was granted.</returns>
    private static string? GetAllowedPrivateHost(HttpRequestMessage? request)
    {
        if (request is not null && request.Options.TryGetValue(OutboundRequestOptions.AllowedPrivateHost, out string? allowedPrivateHost))
            return allowedPrivateHost;
        return null;
    }

    /// <summary>
    /// Connects to the first endpoint that accepts the connection, starting the next address shortly after the previous one, instead of waiting for it to fail,
    /// so an address that silently drops the connection cannot delay the others by the operating system timeout.
    /// </summary>
    /// <param name="endPoints">The validated endpoints of the host.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The stream of the established connection.</returns>
    /// <exception cref="HttpRequestException">None of the endpoints accepts the connection.</exception>
    private static async ValueTask<Stream> ConnectToFirstEndPointAsync(IReadOnlyList<IPEndPoint> endPoints, CancellationToken cancellationToken)
    {
        // Every attempt shares this token, so the winner can cancel the attempts that are still running, while the caller keeps the ability to cancel all of them.
        using CancellationTokenSource attemptCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        List<Task<Stream>> attempts = [];

        // Each address starts a little later than the previous one, so a blackholed address cannot delay the next one by the whole operating system timeout.
        for (int index = 0; index < endPoints.Count; index++)
            attempts.Add(ConnectToEndPointAsync(endPoints[index], index, attemptCancellation.Token));

        Exception? lastException = null;
        try
        {
            // The first attempt that succeeds wins. A failed attempt is removed and the loop keeps waiting for the remaining ones.
            while (attempts.Count > 0)
            {
                Task<Stream> completedAttempt = await Task.WhenAny(attempts).ConfigureAwait(false);
                // Remove the completed attempt before the cleanup below, so only the attempts that are still running are disposed there.
                attempts.Remove(completedAttempt);
                try
                {
                    Stream stream = await completedAttempt.ConfigureAwait(false);
                    // A connection was established. Cancel and dispose every attempt that lost the race before returning the winner's stream.
                    attemptCancellation.Cancel();
                    await DisposeUnusedAttemptsAsync(attempts).ConfigureAwait(false);
                    attempts.Clear();
                    return stream;
                }
                catch (Exception exception) when (exception is SocketException or OperationCanceledException)
                {
                    // One unreachable address must not stop the next one from being tried, but a cancellation of the whole request must propagate.
                    if (cancellationToken.IsCancellationRequested)
                        throw;
                    lastException = exception;
                }
            }
        }
        finally
        {
            // An unexpected exception, or a cancellation, must not leave the other attempts running or their sockets open.
            if (attempts.Count > 0)
            {
                attemptCancellation.Cancel();
                await DisposeUnusedAttemptsAsync(attempts).ConfigureAwait(false);
            }
        }

        // Every validated address was tried and none of them accepted the connection.
        throw new HttpRequestException("None of the validated addresses of the host accepted the connection.", lastException);
    }

    /// <summary>
    /// Connects to a single endpoint, after waiting for its position in the staggered attempts.
    /// </summary>
    /// <param name="endPoint">The endpoint to connect to.</param>
    /// <param name="attemptIndex">The zero based position of the attempt among the endpoints of the host.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The stream of the established connection.</returns>
    private static async Task<Stream> ConnectToEndPointAsync(IPEndPoint endPoint, int attemptIndex, CancellationToken cancellationToken)
    {
        // The first address connects immediately. Each later address waits for its turn in the stagger, giving the earlier address a short head start.
        if (attemptIndex > 0)
            await Task.Delay(ATTEMPT_STAGGER_IN_MILLISECONDS * attemptIndex, cancellationToken).ConfigureAwait(false);

        Socket socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            // Whether the attempt failed or was canceled by a winner, its socket must be released before the exception continues.
            socket.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Disposes the connections established by the attempts that lost the race, and observes their failures so no exception is left unhandled.
    /// </summary>
    /// <param name="attempts">The attempts that are no longer needed.</param>
    /// <returns>A task that completes when every attempt was observed.</returns>
    private static async Task DisposeUnusedAttemptsAsync(List<Task<Stream>> attempts)
    {
        foreach (Task<Stream> attempt in attempts)
        {
            try
            {
                // A losing attempt can still have connected just before it was canceled, so its stream must be disposed too.
                Stream stream = await attempt.ConfigureAwait(false);
                stream.Dispose();
            }
            catch
            {
                // The attempt failed or was canceled, so it has no stream to dispose. Its exception is expected and must not mask the winner.
            }
        }
    }
}
