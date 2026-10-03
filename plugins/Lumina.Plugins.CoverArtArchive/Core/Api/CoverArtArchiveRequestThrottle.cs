#region ========================================================================= USING =====================================================================================
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core.Api;

/// <summary>
/// Class for spacing out the requests sent to the Cover Art Archive API, so the configured minimum request interval is respected across every caller in the process.
/// </summary>
internal sealed class CoverArtArchiveRequestThrottle
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    // The timestamp of the last reserved request slot, shared process wide, so the configured interval is enforced between requests no matter which caller sends them.
    private DateTimeOffset _lastRequestAt = DateTimeOffset.MinValue;

    /// <summary>
    /// Waits until the minimum interval elapsed since the previous reserved request, then reserves the next request slot.
    /// </summary>
    /// <param name="minimumRequestInterval">The minimum amount of time that must elapse between two consecutive requests.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that completes once the caller may send its request.</returns>
    public async Task WaitAsync(TimeSpan minimumRequestInterval, CancellationToken cancellationToken)
    {
        // The gate is held across the delay, so concurrent callers queue up and each one is released only once the interval elapsed since the
        // previous reserved slot.
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // A non positive interval means the callers are not spaced out at all, and it must not be added to the initial timestamp, which would underflow it.
            if (minimumRequestInterval > TimeSpan.Zero)
            {
                DateTimeOffset earliestNextRequest = _lastRequestAt + minimumRequestInterval;
                TimeSpan delay = earliestNextRequest - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            _lastRequestAt = DateTimeOffset.UtcNow;
        }
        finally
        {
            _gate.Release();
        }
    }
}
