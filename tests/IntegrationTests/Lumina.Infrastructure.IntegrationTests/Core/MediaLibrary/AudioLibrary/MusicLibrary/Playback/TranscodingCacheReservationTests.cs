#region ========================================================================= USING =====================================================================================
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
/// Contains integration tests for the <see cref="TranscodingCacheReservation"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingCacheReservationTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly string _cacheRoot;
    private readonly TranscodingCacheCoordinator _sut;
    private readonly NormalizedTrackIndexDtoFixture _normalizedTrackIndexDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheReservationTests"/> class.
    /// </summary>
    public TranscodingCacheReservationTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-transcoding-cache-reservation-{Guid.NewGuid():N}");
        _cacheRoot = Path.Combine(_temporaryDirectory, "cache");
        Directory.CreateDirectory(_cacheRoot);
        _sut = TranscodingCacheCoordinator.AcquireForRoot(_cacheRoot);
    }

    [Fact]
    public void TryUpdate_WhenTheReservationIsUnfinished_ShouldGrowTheReservation()
    {
        // Arrange
        Assert.True(_sut.TryReserve(40, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        try
        {
            // Act
            bool result = reservation.TryUpdate(80, 100, TimeSpan.Zero);

            // Assert
            Assert.True(result);
            Assert.False(_sut.TryReserve(30, 100, TimeSpan.Zero, out _));
        }
        finally
        {
            reservation.Dispose();
        }
    }

    [Fact]
    public void TryUpdate_WhenTheReservationIsCompleted_ShouldReturnFalse()
    {
        // Arrange
        Assert.True(_sut.TryReserve(50, 200, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        Assert.True(reservation.TryComplete(CreateCacheId(), CreateTemporaryOutput(50), 50, _normalizedTrackIndexDtoFixture.Create(), 200, TimeSpan.Zero, out _, out _));

        try
        {
            // Act
            bool result = reservation.TryUpdate(60, 200, TimeSpan.Zero);

            // Assert
            Assert.False(result);
        }
        finally
        {
            reservation.Dispose();
        }
    }

    [Fact]
    public void TryUpdate_WhenTheReservationIsDisposed_ShouldReturnFalse()
    {
        // Arrange
        Assert.True(_sut.TryReserve(50, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        reservation.Dispose();

        // Act
        bool result = reservation.TryUpdate(50, 100, TimeSpan.Zero);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryComplete_WhenTheReservationIsCompleted_ShouldReturnFalse()
    {
        // Arrange
        string cacheId = CreateCacheId();
        NormalizedTrackIndexDto index = _normalizedTrackIndexDtoFixture.Create();
        Assert.True(_sut.TryReserve(50, 200, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        Assert.True(reservation.TryComplete(cacheId, CreateTemporaryOutput(50), 50, index, 200, TimeSpan.Zero, out _, out _));
        string secondTemporaryPath = CreateTemporaryOutput(50);

        try
        {
            // Act
            bool result = reservation.TryComplete(cacheId, secondTemporaryPath, 50, _normalizedTrackIndexDtoFixture.Create(), 200, TimeSpan.Zero, out bool isKeptOverCapacity, out bool isEntryInUse);

            // Assert
            Assert.False(result);
            Assert.False(isKeptOverCapacity);
            Assert.False(isEntryInUse);
            Assert.True(Directory.Exists(secondTemporaryPath));
            Assert.True(_sut.TryGetIndex(cacheId, out NormalizedTrackIndexDto? cachedIndex));
            Assert.Same(index, cachedIndex);
        }
        finally
        {
            reservation.Dispose();
        }
    }

    [Fact]
    public void TryComplete_WhenTheReservationIsDisposed_ShouldReturnFalse()
    {
        // Arrange
        Assert.True(_sut.TryReserve(50, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        reservation.Dispose();
        string temporaryPath = CreateTemporaryOutput(50);

        // Act
        bool result = reservation.TryComplete(CreateCacheId(), temporaryPath, 50, _normalizedTrackIndexDtoFixture.Create(), 100, TimeSpan.Zero, out _, out _);

        // Assert
        Assert.False(result);
        Assert.True(Directory.Exists(temporaryPath));
    }

    [Fact]
    public void Dispose_WhenTheReservationIsNotCompleted_ShouldReleaseTheReservedCapacity()
    {
        // Arrange
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        Assert.False(_sut.TryReserve(100, 100, TimeSpan.Zero, out _));

        // Act
        reservation.Dispose();

        // Assert
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? laterReservation));
        laterReservation!.Dispose();
    }

    [Fact]
    public void Dispose_WhenTheReservationIsCompleted_ShouldKeepThePublishedEntryAccounted()
    {
        // Arrange
        string cacheId = CreateCacheId();
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        Assert.True(reservation.TryComplete(cacheId, CreateTemporaryOutput(100), 100, _normalizedTrackIndexDtoFixture.Create(), 100, TimeSpan.Zero, out _, out _));

        // Act
        reservation.Dispose();
        reservation.Dispose();

        // Assert
        Assert.True(_sut.TryGetIndex(cacheId, out _));
        Assert.False(_sut.TryReserve(100, 100, TimeSpan.FromHours(1), out _));
    }

    [Fact]
    public void Dispose_WhenCalledTwice_ShouldReleaseTheReservedCapacityOnce()
    {
        // Arrange
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);

        // Act
        reservation.Dispose();
        reservation.Dispose();

        // Assert
        Assert.True(_sut.TryReserve(100, 100, TimeSpan.Zero, out TranscodingCacheReservation? laterReservation));
        laterReservation!.Dispose();
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
    /// <returns>The path of the created temporary output directory.</returns>
    private string CreateTemporaryOutput(int size)
    {
        string temporaryPath = Path.Combine(_temporaryDirectory, $"tmp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryPath);
        File.WriteAllBytes(Path.Combine(temporaryPath, "segment.m4s"), new byte[size]);
        return temporaryPath;
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
