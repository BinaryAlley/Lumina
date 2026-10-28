#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Caching;
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains integration tests for the <see cref="TranscodingCacheCoordinator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingCacheCoordinatorTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly string _cacheRoot;
    private readonly TranscodingCacheCoordinator _sut;
    private readonly NormalizedTrackIndexDtoFixture _normalizedTrackIndexDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheCoordinatorTests"/> class.
    /// </summary>
    public TranscodingCacheCoordinatorTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-transcoding-cache-coordinator-{Guid.NewGuid():N}");
        _cacheRoot = Path.Combine(_temporaryDirectory, "cache");
        Directory.CreateDirectory(_cacheRoot);
        _sut = TranscodingCacheCoordinator.AcquireForRoot(_cacheRoot);
    }

    [Fact]
    public void AcquireForRoot_WhenCalledTwiceForTheSameRoot_ShouldReturnTheSameInstance()
    {
        // Arrange
        string cacheRoot = Path.Combine(_temporaryDirectory, "shared-cache");

        // Act
        TranscodingCacheCoordinator firstCoordinator = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);
        TranscodingCacheCoordinator secondCoordinator = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);

        // Assert
        try
        {
            Assert.Same(firstCoordinator, secondCoordinator);
        }
        finally
        {
            firstCoordinator.Release();
            secondCoordinator.Release();
        }
    }

    [Fact]
    public void Release_WhenOneReferenceRemains_ShouldKeepTheCoordinatorRegistered()
    {
        // Arrange
        string cacheRoot = Path.Combine(_temporaryDirectory, "partially-released-cache");
        TranscodingCacheCoordinator firstCoordinator = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);
        TranscodingCacheCoordinator secondCoordinator = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);

        try
        {
            // Act
            firstCoordinator.Release();
            TranscodingCacheCoordinator acquiredAfterRelease = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);

            // Assert
            try
            {
                Assert.Same(firstCoordinator, acquiredAfterRelease);
            }
            finally
            {
                acquiredAfterRelease.Release();
            }
        }
        finally
        {
            firstCoordinator.Release();
            secondCoordinator.Release();
        }
    }

    [Fact]
    public void Release_WhenTheLastReferenceIsReleased_ShouldMakeALaterAcquireReturnANewInstance()
    {
        // Arrange
        string cacheRoot = Path.Combine(_temporaryDirectory, "fully-released-cache");
        TranscodingCacheCoordinator firstCoordinator = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);
        TranscodingCacheCoordinator secondCoordinator = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);

        // Act
        firstCoordinator.Release();
        secondCoordinator.Release();
        TranscodingCacheCoordinator laterCoordinator = TranscodingCacheCoordinator.AcquireForRoot(cacheRoot);

        // Assert
        try
        {
            Assert.NotSame(firstCoordinator, laterCoordinator);
        }
        finally
        {
            laterCoordinator.Release();
        }
    }

    [Theory]
    [InlineData(0)] // a zero sized output
    [InlineData(-1)] // a negative sized output
    public void TryReserve_WhenTheRequestedSizeIsNotPositive_ShouldReturnFalse(int requestedBytes)
    {
        // Act
        bool result = _sut.TryReserve(requestedBytes, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation);

        // Assert
        Assert.False(result);
        Assert.Null(reservation);
    }

    [Theory]
    [InlineData(0)] // a zero maximum cache size
    [InlineData(-1)] // a negative maximum cache size
    public void TryReserve_WhenTheMaximumCacheSizeIsNotPositive_ShouldReturnFalse(int maxCacheBytes)
    {
        // Act
        bool result = _sut.TryReserve(50, maxCacheBytes, TimeSpan.Zero, out TranscodingCacheReservation? reservation);

        // Assert
        Assert.False(result);
        Assert.Null(reservation);
    }

    [Fact]
    public void TryReserve_WhenTheRequestExceedsTheMaximum_ShouldReturnFalse()
    {
        // Act
        bool result = _sut.TryReserve(101, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation);

        // Assert
        Assert.False(result);
        Assert.Null(reservation);
    }

    [Fact]
    public void TryReserve_WhenTheRequestFits_ShouldReserveCapacity()
    {
        // Act
        bool result = _sut.TryReserve(50, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation);

        // Assert
        Assert.True(result);
        Assert.NotNull(reservation);
        reservation!.Dispose();
    }

    [Fact]
    public void TryReserve_WhenTheCacheAlreadyHoldsAReservation_ShouldCountItAgainstTheLimit()
    {
        // Arrange
        Assert.True(_sut.TryReserve(60, 100, TimeSpan.Zero, out TranscodingCacheReservation? firstReservation));
        Assert.NotNull(firstReservation);

        try
        {
            // Act
            bool result = _sut.TryReserve(50, 100, TimeSpan.Zero, out TranscodingCacheReservation? secondReservation);

            // Assert
            Assert.False(result);
            Assert.Null(secondReservation);
        }
        finally
        {
            firstReservation.Dispose();
        }
    }

    [Fact]
    public void TryGrow_WhenTheReservationIsUnknown_ShouldReturnFalse()
    {
        // Act
        bool result = _sut.TryGrow(Guid.NewGuid().ToString("N"), 50, 100, TimeSpan.Zero);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryGrow_WhenTheActualSizeIsNotLargerThanTheReservation_ShouldKeepTheReservedSize()
    {
        // Arrange
        Assert.True(_sut.TryReserve(100, 150, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        try
        {
            // Act
            bool result = _sut.TryGrow(reservation.Id, 50, 150, TimeSpan.Zero);

            // Assert
            Assert.True(result);
            Assert.False(_sut.TryReserve(60, 150, TimeSpan.Zero, out _));
        }
        finally
        {
            reservation!.Dispose();
        }
    }

    [Fact]
    public void TryGrow_WhenTheGrownSizeFits_ShouldIncreaseTheReservation()
    {
        // Arrange
        Assert.True(_sut.TryReserve(40, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        try
        {
            // Act
            bool result = _sut.TryGrow(reservation.Id, 80, 100, TimeSpan.Zero);

            // Assert
            Assert.True(result);
            Assert.False(_sut.TryReserve(30, 100, TimeSpan.Zero, out _));
            Assert.True(_sut.TryReserve(20, 100, TimeSpan.Zero, out TranscodingCacheReservation? remainingReservation));
            remainingReservation!.Dispose();
        }
        finally
        {
            reservation!.Dispose();
        }
    }

    [Fact]
    public void TryGrow_WhenTheGrownSizeExceedsTheMaximum_ShouldReturnFalse()
    {
        // Arrange
        Assert.True(_sut.TryReserve(40, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        try
        {
            // Act
            bool result = _sut.TryGrow(reservation.Id, 101, 100, TimeSpan.Zero);

            // Assert
            Assert.False(result);
        }
        finally
        {
            reservation!.Dispose();
        }
    }

    [Fact]
    public void TryGrow_WhenTheActualSizeIsNegative_ShouldReturnFalse()
    {
        // Arrange
        Assert.True(_sut.TryReserve(40, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        try
        {
            // Act
            bool result = _sut.TryGrow(reservation.Id, -1, 100, TimeSpan.Zero);

            // Assert
            Assert.False(result);
        }
        finally
        {
            reservation!.Dispose();
        }
    }

    [Fact]
    public void TryPublish_WhenTheEntryFits_ShouldMoveTheProducedEntryAndRegisterIt()
    {
        // Arrange
        string cacheId = CreateCacheId();
        string temporaryPath = CreateTemporaryOutput(128);
        NormalizedTrackIndexDto index = _normalizedTrackIndexDtoFixture.Create();
        Assert.True(_sut.TryReserve(128, 1024, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        // Act
        bool result = _sut.TryPublish(reservation.Id, cacheId, temporaryPath, 128, index, 1024, TimeSpan.Zero, out bool isKeptOverCapacity, out bool isEntryInUse);

        // Assert
        Assert.True(result);
        Assert.False(isKeptOverCapacity);
        Assert.False(isEntryInUse);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        Assert.True(Directory.Exists(cachePath));
        Assert.False(Directory.Exists(temporaryPath));
        Assert.Equal(128, TranscodingCacheCoordinator.GetDirectorySize(cachePath));
        Assert.True(_sut.TryGetIndex(cacheId, out NormalizedTrackIndexDto? cachedIndex));
        Assert.Same(index, cachedIndex);
    }

    [Fact]
    public void TryPublish_WhenTheSameCacheIdIsPublishedAgain_ShouldReplaceThePreviousEntry()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 30, 100);
        string replacementPath = CreateTemporaryOutput(40, "replacement.m4s");
        NormalizedTrackIndexDto replacementIndex = _normalizedTrackIndexDtoFixture.Create();
        Assert.True(_sut.TryReserve(40, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        // Act
        bool result = _sut.TryPublish(reservation.Id, cacheId, replacementPath, 40, replacementIndex, 100, TimeSpan.Zero, out bool isKeptOverCapacity, out bool isEntryInUse);

        // Assert
        Assert.True(result);
        Assert.False(isKeptOverCapacity);
        Assert.False(isEntryInUse);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        Assert.True(File.Exists(Path.Combine(cachePath, "replacement.m4s")));
        Assert.False(File.Exists(Path.Combine(cachePath, "segment.m4s")));
        Assert.True(_sut.TryGetIndex(cacheId, out NormalizedTrackIndexDto? cachedIndex));
        Assert.Same(replacementIndex, cachedIndex);
        Assert.True(_sut.TryReserve(60, 100, TimeSpan.FromHours(1), out TranscodingCacheReservation? accountingProbe));
        accountingProbe!.Dispose();
    }

    [Fact]
    public void TryPublish_WhenAReaderHoldsThePreviousEntry_ShouldReturnFalseAndKeepThePreviousEntry()
    {
        // Arrange
        string cacheId = CreateCacheId();
        NormalizedTrackIndexDto originalIndex = PublishEntry(cacheId, 50, 200);
        using (TranscodingCacheReaderLease lease = _sut.AcquireReaderLease(cacheId))
        {
            string temporaryPath = CreateTemporaryOutput(50);
            Assert.True(_sut.TryReserve(50, 200, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
            Assert.NotNull(reservation);

            // Act
            bool result = _sut.TryPublish(reservation.Id, cacheId, temporaryPath, 50, _normalizedTrackIndexDtoFixture.Create(), 200, TimeSpan.Zero, out bool isKeptOverCapacity, out bool isEntryInUse);

            // Assert
            Assert.False(result);
            Assert.True(isEntryInUse);
            Assert.False(isKeptOverCapacity);
            Assert.True(Directory.Exists(temporaryPath));
            Assert.True(Directory.Exists(Path.Combine(_cacheRoot, cacheId)));
            Assert.True(_sut.TryGetIndex(cacheId, out NormalizedTrackIndexDto? cachedIndex));
            Assert.Same(originalIndex, cachedIndex);
        }
    }

    [Fact]
    public void TryPublish_WhenTheProducedEntryAloneExceedsTheMaximum_ShouldKeepItOverCapacity()
    {
        // Arrange
        string cacheId = CreateCacheId();
        string temporaryPath = CreateTemporaryOutput(150);
        Assert.True(_sut.TryReserve(50, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        // Act
        bool result = _sut.TryPublish(reservation.Id, cacheId, temporaryPath, 150, _normalizedTrackIndexDtoFixture.Create(), 100, TimeSpan.Zero, out bool isKeptOverCapacity, out bool isEntryInUse);

        // Assert
        Assert.True(result);
        Assert.True(isKeptOverCapacity);
        Assert.False(isEntryInUse);
        Assert.True(Directory.Exists(Path.Combine(_cacheRoot, cacheId)));
        Assert.True(_sut.TryGetIndex(cacheId, out _));
    }

    [Fact]
    public void TryPublish_WhenTheTemporaryDirectoryDoesNotExist_ShouldReturnFalseAndConsumeTheReservation()
    {
        // Arrange
        Assert.True(_sut.TryReserve(50, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        string missingTemporaryPath = Path.Combine(_temporaryDirectory, "missing-output");

        // Act
        bool result = _sut.TryPublish(reservation.Id, CreateCacheId(), missingTemporaryPath, 50, _normalizedTrackIndexDtoFixture.Create(), 100, TimeSpan.Zero, out bool isKeptOverCapacity, out bool isEntryInUse);

        // Assert
        Assert.False(result);
        Assert.False(isKeptOverCapacity);
        Assert.False(isEntryInUse);
        Assert.False(reservation.TryUpdate(50, 100, TimeSpan.Zero));
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? replacementReservation));
        replacementReservation!.Dispose();
    }

    [Fact]
    public void ReleaseReservation_WhenCalled_ShouldFreeTheReservedCapacity()
    {
        // Arrange
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? firstReservation));
        Assert.NotNull(firstReservation);
        Assert.False(_sut.TryReserve(100, 100, TimeSpan.Zero, out _));

        // Act
        _sut.ReleaseReservation(firstReservation.Id);

        // Assert
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? secondReservation));
        secondReservation!.Dispose();
        firstReservation.Dispose();
    }

    [Fact]
    public void Touch_WhenTheCacheIdIsUnknown_ShouldBeIgnored()
    {
        // Act
        Exception? exception = Record.Exception(() => _sut.Touch(CreateCacheId(), TimeSpan.Zero));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Touch_WhenTheThrottleIntervalHasNotElapsed_ShouldNotChangeTheEntry()
    {
        // Arrange
        string cacheId = CreateCacheId();
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        PublishEntry(cacheId, 50, 100);
        Thread.Sleep(200);

        // Act
        _sut.Touch(cacheId, TimeSpan.FromDays(1));
        bool result = _sut.TryReserve(80, 100, TimeSpan.FromMilliseconds(100), out _);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(cachePath));
    }

    [Fact]
    public void Touch_WhenTheThrottleIntervalHasElapsed_ShouldUpdateTheEntry()
    {
        // Arrange
        string cacheId = CreateCacheId();
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        PublishEntry(cacheId, 50, 100);
        Thread.Sleep(200);

        // Act
        _sut.Touch(cacheId, TimeSpan.Zero);
        bool result = _sut.TryReserve(80, 100, TimeSpan.FromMilliseconds(100), out _);

        // Assert
        Assert.False(result);
        Assert.True(Directory.Exists(cachePath));
    }

    [Fact]
    public void TryReserve_WhenTheOnlyEntryIsLeased_ShouldNotEvictItUntilTheLeaseIsReleased()
    {
        // Arrange
        string cacheId = CreateCacheId();
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        PublishEntry(cacheId, 50, 100);
        TranscodingCacheReaderLease lease = _sut.AcquireReaderLease(cacheId);

        try
        {
            // Act
            bool result = _sut.TryReserve(80, 100, TimeSpan.Zero, out _);

            // Assert
            Assert.False(result);
            Assert.True(Directory.Exists(cachePath));
        }
        finally
        {
            lease.Dispose();
        }
        Assert.False(Directory.Exists(cachePath));
        Assert.True(_sut.TryReserve(80, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        reservation!.Dispose();
    }

    [Fact]
    public void TryReserve_WhenTheCacheIsFull_ShouldEvictTheLeastRecentlyUsedEntriesInOrder()
    {
        // Arrange
        string firstCacheId = CreateCacheId();
        string secondCacheId = CreateCacheId();
        string thirdCacheId = CreateCacheId();
        PublishEntry(firstCacheId, 50, 150);
        Thread.Sleep(30);
        PublishEntry(secondCacheId, 50, 150);
        Thread.Sleep(30);
        PublishEntry(thirdCacheId, 50, 150);

        // Act
        bool result = _sut.TryReserve(60, 150, TimeSpan.Zero, out TranscodingCacheReservation? reservation);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(Path.Combine(_cacheRoot, firstCacheId)));
        Assert.False(Directory.Exists(Path.Combine(_cacheRoot, secondCacheId)));
        Assert.True(Directory.Exists(Path.Combine(_cacheRoot, thirdCacheId)));
        reservation!.Dispose();
    }

    [Fact]
    public void TryReserve_WhenAnEntryIsInsideTheGracePeriod_ShouldNotEvictIt()
    {
        // Arrange
        string cacheId = CreateCacheId();
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        PublishEntry(cacheId, 50, 100);

        // Act
        bool result = _sut.TryReserve(80, 100, TimeSpan.FromHours(1), out TranscodingCacheReservation? reservation);

        // Assert
        Assert.False(result);
        Assert.Null(reservation);
        Assert.True(Directory.Exists(cachePath));
    }

    [Fact]
    public void TryReserve_WhenTheGracePeriodHasElapsed_ShouldEvictTheEntry()
    {
        // Arrange
        string cacheId = CreateCacheId();
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        PublishEntry(cacheId, 50, 100);

        // Act
        bool result = _sut.TryReserve(80, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(cachePath));
        reservation!.Dispose();
    }

    [Fact]
    public void TryPublish_WhenOnlyUnprotectedEntriesAreLeased_ShouldNeverEvictThePublishedEntry()
    {
        // Arrange
        string firstCacheId = CreateCacheId();
        string secondCacheId = CreateCacheId();
        string publishedCacheId = CreateCacheId();
        PublishEntry(firstCacheId, 50, 200);
        PublishEntry(secondCacheId, 50, 200);
        Assert.True(_sut.TryReserve(50, 200, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        TranscodingCacheReaderLease firstLease = _sut.AcquireReaderLease(firstCacheId);
        TranscodingCacheReaderLease secondLease = _sut.AcquireReaderLease(secondCacheId);

        try
        {
            // Act
            bool result = _sut.TryPublish(reservation.Id, publishedCacheId, CreateTemporaryOutput(50), 50, _normalizedTrackIndexDtoFixture.Create(), 100, TimeSpan.Zero, out bool isKeptOverCapacity, out bool isEntryInUse);

            // Assert
            Assert.True(result);
            Assert.True(isKeptOverCapacity);
            Assert.False(isEntryInUse);
            Assert.True(Directory.Exists(Path.Combine(_cacheRoot, publishedCacheId)));
        }
        finally
        {
            firstLease.Dispose();
            secondLease.Dispose();
        }
        Assert.False(Directory.Exists(Path.Combine(_cacheRoot, firstCacheId)));
        Assert.False(Directory.Exists(Path.Combine(_cacheRoot, secondCacheId)));
        Assert.True(Directory.Exists(Path.Combine(_cacheRoot, publishedCacheId)));
    }

    [Fact]
    public void TryReserve_WhenTheRootAlreadyHoldsEntries_ShouldAccountForThem()
    {
        // Arrange
        string existingCacheId = CreateCacheId();
        string entryPath = Path.Combine(_cacheRoot, existingCacheId);
        Directory.CreateDirectory(entryPath);
        File.WriteAllBytes(Path.Combine(entryPath, "segment.m4s"), new byte[60]);

        // Act
        bool result = _sut.TryReserve(50, 100, TimeSpan.FromHours(1), out TranscodingCacheReservation? reservation);

        // Assert
        Assert.False(result);
        Assert.Null(reservation);
    }

    [Fact]
    public void TryReserve_WhenTheRootHoldsNamesThatAreNotCacheIds_ShouldIgnoreThem()
    {
        // Arrange
        string unrelatedDirectoryPath = Path.Combine(_cacheRoot, "not-a-cache-id");
        Directory.CreateDirectory(unrelatedDirectoryPath);
        File.WriteAllBytes(Path.Combine(unrelatedDirectoryPath, "payload.bin"), new byte[200]);

        // Act
        bool result = _sut.TryReserve(100, 100, TimeSpan.FromHours(1), out TranscodingCacheReservation? reservation);

        // Assert
        Assert.True(result);
        reservation!.Dispose();
    }

    [Fact]
    public void TryReserve_WhenTheRootHoldsTombstones_ShouldHandThemToTheSweeper()
    {
        // Arrange
        string tombstonePath = Path.Combine(_cacheRoot, TranscodingCachePaths.DELETION_DIRECTORY_PREFIX + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tombstonePath);
        File.WriteAllBytes(Path.Combine(tombstonePath, "segment.m4s"), new byte[200]);

        // Act
        bool result = _sut.TryReserve(100, 100, TimeSpan.FromHours(1), out TranscodingCacheReservation? reservation);

        // Assert
        Assert.True(result);
        reservation!.Dispose();
        Assert.True(WaitUntil(() => !Directory.Exists(tombstonePath), TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public void GetDirectorySize_WhenTheDirectoryHasNestedFiles_ShouldReturnTheirTotalSize()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "size-root");
        Directory.CreateDirectory(Path.Combine(directoryPath, "nested", "deeper"));
        File.WriteAllBytes(Path.Combine(directoryPath, "first.bin"), new byte[10]);
        File.WriteAllBytes(Path.Combine(directoryPath, "nested", "second.bin"), new byte[20]);
        File.WriteAllBytes(Path.Combine(directoryPath, "nested", "deeper", "third.bin"), new byte[30]);

        // Act
        long result = TranscodingCacheCoordinator.GetDirectorySize(directoryPath);

        // Assert
        Assert.Equal(60, result);
    }

    [Fact]
    public void GetDirectorySize_WhenTheDirectoryDoesNotExist_ShouldReturnZero()
    {
        // Arrange
        string directoryPath = Path.Combine(_temporaryDirectory, "missing-size-root");

        // Act
        long result = TranscodingCacheCoordinator.GetDirectorySize(directoryPath);

        // Assert
        Assert.Equal(0, result);
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
    /// Creates a temporary output directory with a single file of the requested size.
    /// </summary>
    /// <param name="size">The size of the produced file, in bytes.</param>
    /// <param name="fileName">The name of the produced file.</param>
    /// <returns>The path of the created temporary output directory.</returns>
    private string CreateTemporaryOutput(int size, string fileName = "segment.m4s")
    {
        string temporaryPath = Path.Combine(_temporaryDirectory, TranscodingCachePaths.TEMPORARY_DIRECTORY_PREFIX + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryPath);
        File.WriteAllBytes(Path.Combine(temporaryPath, fileName), new byte[size]);
        return temporaryPath;
    }

    /// <summary>
    /// Reserves capacity and publishes a produced entry under the requested cache identifier.
    /// </summary>
    /// <param name="cacheId">The identifier of the produced cache entry.</param>
    /// <param name="size">The total size of the produced cache entry, in bytes.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    /// <returns>The index that was published with the entry.</returns>
    private NormalizedTrackIndexDto PublishEntry(string cacheId, int size, long maxCacheBytes)
    {
        string temporaryPath = CreateTemporaryOutput(size);
        NormalizedTrackIndexDto index = _normalizedTrackIndexDtoFixture.Create();
        Assert.True(_sut.TryReserve(size, maxCacheBytes, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        Assert.True(_sut.TryPublish(reservation.Id, cacheId, temporaryPath, size, index, maxCacheBytes, TimeSpan.Zero, out _, out _));
        return index;
    }

    /// <summary>
    /// Waits, for a bounded period, until the condition becomes true.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <param name="timeout">The maximum period to wait.</param>
    /// <returns><see langword="true"/> when the condition became true within the timeout, otherwise <see langword="false"/>.</returns>
    private static bool WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return true;
            Thread.Sleep(20);
        }
        return condition();
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
