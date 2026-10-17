#region ========================================================================= USING =====================================================================================
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// A read stream over a cached segment that keeps the reader lease of its cache entry until the stream is disposed.
/// </summary>
internal sealed class TranscodingCacheLeaseStream : Stream
{
    private readonly Stream _innerStream;
    private readonly TranscodingCacheReaderLease _lease;
    private bool _isDisposed;

    /// <summary>
    /// Gets whether the stream supports reading.
    /// </summary>
    public override bool CanRead => _innerStream.CanRead;

    /// <summary>
    /// Gets whether the stream supports seeking.
    /// </summary>
    public override bool CanSeek => _innerStream.CanSeek;

    /// <summary>
    /// Gets whether the stream supports writing.
    /// </summary>
    public override bool CanWrite => _innerStream.CanWrite;

    /// <summary>
    /// Gets the length of the stream, in bytes.
    /// </summary>
    public override long Length => _innerStream.Length;

    /// <summary>
    /// Gets or sets the position within the stream.
    /// </summary>
    public override long Position
    {
        get { return _innerStream.Position; }
        set { _innerStream.Position = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheLeaseStream"/> class.
    /// </summary>
    /// <param name="innerStream">The stream over the cached segment.</param>
    /// <param name="lease">The reader lease that protects the cache entry of the segment.</param>
    internal TranscodingCacheLeaseStream(Stream innerStream, TranscodingCacheReaderLease lease)
    {
        _innerStream = innerStream;
        _lease = lease;
    }

    /// <summary>
    /// Clears the buffers of the stream.
    /// </summary>
    public override void Flush()
    {
        _innerStream.Flush();
    }

    /// <summary>
    /// Reads a block of bytes from the stream.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <param name="offset">The offset in the buffer at which to start writing.</param>
    /// <param name="count">The maximum number of bytes to read.</param>
    /// <returns>The number of bytes read.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        return _innerStream.Read(buffer, offset, count);
    }

    /// <summary>
    /// Reads a block of bytes from the stream.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <returns>The number of bytes read.</returns>
    public override int Read(Span<byte> buffer)
    {
        return _innerStream.Read(buffer);
    }

    /// <summary>
    /// Reads a block of bytes from the stream asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <param name="offset">The offset in the buffer at which to start writing.</param>
    /// <param name="count">The maximum number of bytes to read.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The number of bytes read.</returns>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    /// <summary>
    /// Reads a block of bytes from the stream asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The number of bytes read.</returns>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _innerStream.ReadAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Sets the position within the stream.
    /// </summary>
    /// <param name="offset">The offset relative to <paramref name="origin"/>.</param>
    /// <param name="origin">The reference point used to obtain the new position.</param>
    /// <returns>The new position within the stream.</returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
        return _innerStream.Seek(offset, origin);
    }

    /// <summary>
    /// Sets the length of the stream.
    /// </summary>
    /// <param name="value">The new length of the stream, in bytes.</param>
    public override void SetLength(long value)
    {
        _innerStream.SetLength(value);
    }

    /// <summary>
    /// Writes a block of bytes to the stream.
    /// </summary>
    /// <param name="buffer">The buffer to write from.</param>
    /// <param name="offset">The offset in the buffer at which to start reading.</param>
    /// <param name="count">The number of bytes to write.</param>
    public override void Write(byte[] buffer, int offset, int count)
    {
        _innerStream.Write(buffer, offset, count);
    }

    /// <summary>
    /// Writes a block of bytes to the stream.
    /// </summary>
    /// <param name="buffer">The buffer to write from.</param>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _innerStream.Write(buffer);
    }

    /// <summary>
    /// Writes a block of bytes to the stream asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to write from.</param>
    /// <param name="offset">The offset in the buffer at which to start reading.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    /// <summary>
    /// Writes a block of bytes to the stream asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to write from.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _innerStream.WriteAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Releases the inner stream and the reader lease.
    /// </summary>
    /// <param name="disposing">Whether the method is called from the <see cref="IDisposable.Dispose"/> method.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _isDisposed = true;
            try
            {
                _innerStream.Dispose();
            }
            finally
            {
                // The lease must be released even when disposing the stream throws, otherwise the entry stays protected from eviction forever.
                _lease.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Releases the inner stream and the reader lease asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public override async ValueTask DisposeAsync()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            try
            {
                await _innerStream.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                // The lease must be released even when disposing the stream throws, otherwise the entry stays protected from eviction forever.
                _lease.Dispose();
            }
        }
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
