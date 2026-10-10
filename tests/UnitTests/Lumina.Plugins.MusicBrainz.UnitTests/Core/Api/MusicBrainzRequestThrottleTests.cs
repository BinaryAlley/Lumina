#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Core.Api;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Core.Api;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzRequestThrottle"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzRequestThrottleTests
{
    private readonly MusicBrainzRequestThrottle _sut = new();

    [Fact]
    public async Task WaitAsync_WhenTheMinimumIntervalHasNotElapsed_ShouldDelayTheSecondRequest()
    {
        // Arrange
        TimeSpan minimumRequestInterval = TimeSpan.FromMilliseconds(200);

        // Act
        await _sut.WaitAsync(minimumRequestInterval, CancellationToken.None);
        Stopwatch stopwatch = Stopwatch.StartNew();
        await _sut.WaitAsync(minimumRequestInterval, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        // The second request must wait for the remainder of the interval, which is close to the configured interval.
        Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(150), $"The elapsed time {stopwatch.Elapsed} was shorter than the expected minimum interval.");
    }

    [Fact]
    public async Task WaitAsync_WhenTheMinimumIntervalIsZero_ShouldNotDelayTheRequests()
    {
        // Arrange
        TimeSpan minimumRequestInterval = TimeSpan.Zero;

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        await _sut.WaitAsync(minimumRequestInterval, CancellationToken.None);
        await _sut.WaitAsync(minimumRequestInterval, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(500), $"The elapsed time {stopwatch.Elapsed} shows an unexpected delay.");
    }

    [Fact]
    public async Task WaitAsync_WhenTheMinimumIntervalIsNegative_ShouldNotDelayTheRequests()
    {
        // Arrange
        TimeSpan minimumRequestInterval = TimeSpan.FromSeconds(-1);

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        await _sut.WaitAsync(minimumRequestInterval, CancellationToken.None);
        await _sut.WaitAsync(minimumRequestInterval, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(500), $"The elapsed time {stopwatch.Elapsed} shows an unexpected delay.");
    }

    [Fact]
    public async Task WaitAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using (CancellationTokenSource cancellationTokenSource = new())
        {
            cancellationTokenSource.Cancel();

            // Act
            Task Act()
            {
                return _sut.WaitAsync(TimeSpan.FromMilliseconds(100), cancellationTokenSource.Token);
            }

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
        }
    }

    [Fact]
    public async Task WaitAsync_WhenCalledConcurrently_ShouldSpaceOutEveryRequest()
    {
        // Arrange
        TimeSpan minimumRequestInterval = TimeSpan.FromMilliseconds(150);

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(
            _sut.WaitAsync(minimumRequestInterval, CancellationToken.None),
            _sut.WaitAsync(minimumRequestInterval, CancellationToken.None),
            _sut.WaitAsync(minimumRequestInterval, CancellationToken.None));
        stopwatch.Stop();

        // Assert
        // Three requests spaced by the interval take at least two intervals in total.
        Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(250), $"The elapsed time {stopwatch.Elapsed} was shorter than the expected total interval.");
    }
}
