#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains integration tests for the <see cref="TranscodingCacheReaderLease"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingCacheReaderLeaseTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly string _cacheRoot;
    private readonly TranscodingCacheCoordinator _sut;
    private readonly NormalizedTrackIndexDtoFixture _normalizedTrackIndexDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheReaderLeaseTests"/> class.
    /// </summary>
    public TranscodingCacheReaderLeaseTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-transcoding-cache-lease-{Guid.NewGuid():N}");
        _cacheRoot = Path.Combine(_temporaryDirectory, "cache");
        Directory.CreateDirectory(_cacheRoot);
        _sut = TranscodingCacheCoordinator.AcquireForRoot(_cacheRoot);
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldReleaseTheLeaseSoTheEntryCanBeEvicted()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 50, 100);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        TranscodingCacheReaderLease lease = _sut.AcquireReaderLease(cacheId);
        Assert.False(_sut.TryReserve(80, 100, TimeSpan.Zero, out _));
        Assert.True(Directory.Exists(cachePath));

        // Act
        lease.Dispose();

        // Assert
        Assert.False(Directory.Exists(cachePath));
        Assert.True(_sut.TryReserve(80, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        reservation!.Dispose();
    }

    [Fact]
    public void Dispose_WhenCalledTwiceAcrossTwoLeases_ShouldReleaseOnlyOncePerLease()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 50, 100);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        TranscodingCacheReaderLease firstLease = _sut.AcquireReaderLease(cacheId);
        TranscodingCacheReaderLease secondLease = _sut.AcquireReaderLease(cacheId);

        try
        {
            // Act
            firstLease.Dispose();
            firstLease.Dispose();

            // Assert
            Assert.False(_sut.TryReserve(80, 100, TimeSpan.Zero, out _));
            Assert.True(Directory.Exists(cachePath));
        }
        finally
        {
            firstLease.Dispose();
            secondLease.Dispose();
        }
        Assert.False(Directory.Exists(cachePath));
    }

    /// <summary>
    /// Creates a well formed opaque cache identifier.
    /// </summary>
    /// <returns>A 64 character lowercase hexadecimal identifier.</returns>
    private static string CreateCacheId()
    {
        return $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Reserves capacity and publishes a produced entry under the requested cache identifier.
    /// </summary>
    /// <param name="cacheId">The identifier of the produced cache entry.</param>
    /// <param name="size">The total size of the produced cache entry, in bytes.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    private void PublishEntry(string cacheId, int size, long maxCacheBytes)
    {
        string temporaryPath = Path.Combine(_temporaryDirectory, $"tmp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryPath);
        File.WriteAllBytes(Path.Combine(temporaryPath, "segment.m4s"), new byte[size]);
        Assert.True(_sut.TryReserve(size, maxCacheBytes, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        Assert.True(_sut.TryPublish(reservation.Id, cacheId, temporaryPath, size, _normalizedTrackIndexDtoFixture.Create(), maxCacheBytes, TimeSpan.Zero, out _, out _));
    }

    /// <summary>
    /// Releases the coordinator and removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        _sut.Release();
        try
        {
            if (Directory.Exists(_temporaryDirectory))
                Directory.Delete(_temporaryDirectory, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // A background sweeper can still be removing a tombstone that was detached by the test.
        }
        GC.SuppressFinalize(this);
    }
}
