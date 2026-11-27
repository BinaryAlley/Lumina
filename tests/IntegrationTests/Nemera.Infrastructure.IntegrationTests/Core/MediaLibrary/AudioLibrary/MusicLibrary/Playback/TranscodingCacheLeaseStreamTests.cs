#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains integration tests for the <see cref="TranscodingCacheLeaseStream"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingCacheLeaseStreamTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly string _cacheRoot;
    private readonly TranscodingCacheCoordinator _coordinator;
    private readonly NormalizedTrackIndexDtoFixture _normalizedTrackIndexDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheLeaseStreamTests"/> class.
    /// </summary>
    public TranscodingCacheLeaseStreamTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-transcoding-cache-stream-{Guid.NewGuid():N}");
        _cacheRoot = Path.Combine(_temporaryDirectory, "cache");
        Directory.CreateDirectory(_cacheRoot);
        _coordinator = TranscodingCacheCoordinator.AcquireForRoot(_cacheRoot);
    }

    [Fact]
    public void CanRead_WhenAccessed_ShouldReturnTheInnerValue()
    {
        // Arrange
        Stream innerStream = Substitute.For<Stream>();
        innerStream.CanRead.Returns(true);

        // Act
        bool result;
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
            result = leaseStream.CanRead;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSeek_WhenAccessed_ShouldReturnTheInnerValue()
    {
        // Arrange
        Stream innerStream = Substitute.For<Stream>();
        innerStream.CanSeek.Returns(false);

        // Act
        bool result;
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
            result = leaseStream.CanSeek;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanWrite_WhenAccessed_ShouldReturnTheInnerValue()
    {
        // Arrange
        Stream innerStream = Substitute.For<Stream>();
        innerStream.CanWrite.Returns(true);

        // Act
        bool result;
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
            result = leaseStream.CanWrite;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Length_WhenAccessed_ShouldReturnTheInnerValue()
    {
        // Arrange
        using (MemoryStream innerStream = new([1, 2, 3, 4, 5]))
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            // Act
            long result = leaseStream.Length;

            // Assert
            Assert.Equal(5L, result);
        }
    }

    [Fact]
    public void Position_WhenAccessed_ShouldReturnAndSetTheInnerValue()
    {
        // Arrange
        using (MemoryStream innerStream = new([1, 2, 3, 4, 5, 6, 7, 8]))
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            innerStream.Position = 5L;

            // Act
            long position = leaseStream.Position;
            leaseStream.Position = 7L;

            // Assert
            Assert.Equal(5L, position);
            Assert.Equal(7L, innerStream.Position);
        }
    }

    [Fact]
    public void Flush_WhenCalled_ShouldFlushTheInnerStream()
    {
        // Arrange
        Stream innerStream = Substitute.For<Stream>();

        // Act
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
            leaseStream.Flush();

        // Assert
        innerStream.Received(1).Flush();
    }

    [Fact]
    public void Read_WhenCalled_ShouldReadFromTheInnerStream()
    {
        // Arrange
        byte[] buffer = new byte[8];
        using (MemoryStream innerStream = new([1, 2, 3, 4, 5]))
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            // Act
            int result = leaseStream.Read(buffer, 1, 4);

            // Assert
            Assert.Equal(4, result);
            Assert.Equal(new byte[] { 0, 1, 2, 3, 4, 0, 0, 0 }, buffer);
            Assert.Equal(4L, innerStream.Position);
        }
    }

    [Fact]
    public void Read_WhenCalledWithASpan_ShouldReadFromTheInnerStream()
    {
        // Arrange
        using (MemoryStream innerStream = new([1, 2, 3, 4]))
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            Span<byte> buffer = stackalloc byte[2];

            // Act
            int result = leaseStream.Read(buffer);

            // Assert
            Assert.Equal(2, result);
            Assert.Equal(1, buffer[0]);
            Assert.Equal(2, buffer[1]);
        }
    }

    [Fact]
    public async Task ReadAsync_WhenCalledWithMemory_ShouldReadFromTheInnerStream()
    {
        // Arrange
        byte[] buffer = new byte[8];
        using (MemoryStream innerStream = new([4, 5, 6, 7]))
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            // Act
            int result = await leaseStream.ReadAsync(buffer.AsMemory(), CancellationToken.None);

            // Assert
            Assert.Equal(4, result);
            Assert.Equal(new byte[] { 4, 5, 6, 7, 0, 0, 0, 0 }, buffer);
            Assert.Equal(4L, innerStream.Position);
        }
    }

    [Fact]
    public void Seek_WhenCalled_ShouldSeekTheInnerStream()
    {
        // Arrange
        using (MemoryStream innerStream = new([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]))
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            // Act
            long result = leaseStream.Seek(3L, SeekOrigin.Begin);

            // Assert
            Assert.Equal(3L, result);
            Assert.Equal(3L, innerStream.Position);
        }
    }

    [Fact]
    public void SetLength_WhenCalled_ShouldSetTheInnerLength()
    {
        // Arrange
        using (MemoryStream innerStream = new())
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            // Act
            leaseStream.SetLength(10L);

            // Assert
            Assert.Equal(10L, innerStream.Length);
        }
    }

    [Fact]
    public void Write_WhenCalled_ShouldWriteToTheInnerStream()
    {
        // Arrange
        using (MemoryStream innerStream = new())
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            byte[] buffer = [1, 2, 3, 4, 5, 6];

            // Act
            leaseStream.Write(buffer, 1, 4);

            // Assert
            Assert.Equal(new byte[] { 2, 3, 4, 5 }, innerStream.ToArray());
        }
    }

    [Fact]
    public void Write_WhenCalledWithASpan_ShouldWriteToTheInnerStream()
    {
        // Arrange
        using (MemoryStream innerStream = new())
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            // Act
            leaseStream.Write([5, 6, 7]);

            // Assert
            Assert.Equal(3L, innerStream.Length);
            Assert.Equal(new byte[] { 5, 6, 7 }, innerStream.ToArray());
        }
    }

    [Fact]
    public async Task WriteAsync_WhenCalledWithMemory_ShouldWriteToTheInnerStream()
    {
        // Arrange
        using (MemoryStream innerStream = new())
        using (TranscodingCacheLeaseStream leaseStream = CreateSut(innerStream))
        {
            // Act
            await leaseStream.WriteAsync(new byte[] { 1, 2 }.AsMemory(), CancellationToken.None);

            // Assert
            Assert.Equal(new byte[] { 1, 2 }, innerStream.ToArray());
        }
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldDisposeTheInnerStreamAndReleaseTheLease()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 50, 100);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        TranscodingCacheReaderLease lease = _coordinator.AcquireReaderLease(cacheId);
        using (MemoryStream innerStream = new())
        {
            TranscodingCacheLeaseStream leaseStream = new(innerStream, lease);
            Assert.False(_coordinator.TryReserve(80, 100, TimeSpan.Zero, out _));
            Assert.True(Directory.Exists(cachePath));

            // Act
            leaseStream.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() =>
            {
                _ = innerStream.Length;
            });
            Assert.False(Directory.Exists(cachePath));
            Assert.True(_coordinator.TryReserve(80, 100, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
            reservation!.Dispose();
        }
    }

    [Fact]
    public void Dispose_WhenCalledTwice_ShouldDisposeTheInnerStreamAndReleaseTheLeaseOnce()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 50, 100);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        TranscodingCacheReaderLease firstLease = _coordinator.AcquireReaderLease(cacheId);
        TranscodingCacheReaderLease secondLease = _coordinator.AcquireReaderLease(cacheId);
        using (MemoryStream firstInnerStream = new())
        using (MemoryStream secondInnerStream = new())
        {
            TranscodingCacheLeaseStream firstStream = new(firstInnerStream, firstLease);
            TranscodingCacheLeaseStream secondStream = new(secondInnerStream, secondLease);

            // Act
            firstStream.Dispose();
            firstStream.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() =>
            {
                _ = firstInnerStream.Length;
            });
            Assert.Equal(0L, secondInnerStream.Length);
            Assert.False(_coordinator.TryReserve(80, 100, TimeSpan.Zero, out _));
            Assert.True(Directory.Exists(cachePath));

            secondStream.Dispose();

            Assert.False(Directory.Exists(cachePath));
            Assert.Throws<ObjectDisposedException>(() =>
            {
                _ = secondInnerStream.Length;
            });
        }
    }

    [Fact]
    public void Dispose_WhenDisposingTheInnerStreamThrows_ShouldStillReleaseTheLease()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 50, 100);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        TranscodingCacheReaderLease lease = _coordinator.AcquireReaderLease(cacheId);
        Stream innerStream = new ThrowingDisposeStream(new IOException("The inner stream cannot be disposed."));
        TranscodingCacheLeaseStream leaseStream = new(innerStream, lease);
        Assert.False(_coordinator.TryReserve(80, 100, TimeSpan.Zero, out _));

        // Act
        Assert.Throws<IOException>(() => leaseStream.Dispose());

        // Assert
        Assert.False(Directory.Exists(cachePath));
    }

    [Fact]
    public async Task DisposeAsync_WhenCalled_ShouldDisposeTheInnerStreamAndReleaseTheLease()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 50, 100);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        TranscodingCacheReaderLease lease = _coordinator.AcquireReaderLease(cacheId);
        Stream innerStream = Substitute.For<Stream>();
        TranscodingCacheLeaseStream leaseStream = new(innerStream, lease);
        Assert.False(_coordinator.TryReserve(80, 100, TimeSpan.Zero, out _));

        // Act
        await leaseStream.DisposeAsync();

        // Assert
        await innerStream.Received(1).DisposeAsync();
        Assert.False(Directory.Exists(cachePath));
    }

    [Fact]
    public async Task DisposeAsync_WhenDisposingTheInnerStreamThrows_ShouldStillReleaseTheLease()
    {
        // Arrange
        string cacheId = CreateCacheId();
        PublishEntry(cacheId, 50, 100);
        string cachePath = Path.Combine(_cacheRoot, cacheId);
        TranscodingCacheReaderLease lease = _coordinator.AcquireReaderLease(cacheId);
        Stream innerStream = new ThrowingDisposeStream(new IOException("The inner stream cannot be disposed."));
        TranscodingCacheLeaseStream leaseStream = new(innerStream, lease);
        Assert.False(_coordinator.TryReserve(80, 100, TimeSpan.Zero, out _));

        // Act
        await Assert.ThrowsAsync<IOException>(async () => await leaseStream.DisposeAsync());

        // Assert
        Assert.False(Directory.Exists(cachePath));
    }

    /// <summary>
    /// Creates the stream under test over the provided inner stream.
    /// </summary>
    /// <param name="innerStream">The stream over the cached segment.</param>
    /// <returns>The created <see cref="TranscodingCacheLeaseStream"/>.</returns>
    private TranscodingCacheLeaseStream CreateSut(Stream innerStream)
    {
        TranscodingCacheReaderLease lease = _coordinator.AcquireReaderLease(CreateCacheId());
        return new TranscodingCacheLeaseStream(innerStream, lease);
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
        Assert.True(_coordinator.TryReserve(size, maxCacheBytes, TimeSpan.Zero, out TranscodingCacheReservation? reservation));
        Assert.NotNull(reservation);
        Assert.True(_coordinator.TryPublish(reservation.Id, cacheId, temporaryPath, size, _normalizedTrackIndexDtoFixture.Create(), maxCacheBytes, TimeSpan.Zero, out _, out _));
    }

    /// <summary>
    /// Releases the coordinator and removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        _coordinator.Release();
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

    /// <summary>
    /// A memory stream whose disposal always fails, used to verify that a cache entry lease is released even when the inner stream cannot be disposed.
    /// </summary>
    private sealed class ThrowingDisposeStream : MemoryStream
    {
        private readonly IOException _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingDisposeStream"/> class.
        /// </summary>
        /// <param name="exception">The exception thrown when the stream is disposed.</param>
        internal ThrowingDisposeStream(IOException exception)
        {
            _exception = exception;
        }

        /// <summary>
        /// Releases the resources of the stream and then throws the configured exception.
        /// </summary>
        /// <param name="disposing">Whether the method is called from the <see cref="IDisposable.Dispose"/> method.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            throw _exception;
        }

        /// <summary>
        /// Gets a failed value task that carries the configured exception.
        /// </summary>
        /// <returns>A failed <see cref="ValueTask"/>.</returns>
        public override ValueTask DisposeAsync()
        {
            return ValueTask.FromException(_exception);
        }
    }
}
