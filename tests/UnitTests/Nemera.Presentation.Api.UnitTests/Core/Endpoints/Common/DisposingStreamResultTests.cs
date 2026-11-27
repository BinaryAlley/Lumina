#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Common;

/// <summary>
/// Contains unit tests for the <see cref="DisposingStreamResult"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DisposingStreamResultTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisposingStreamResultTests"/> class.
    /// </summary>
    public DisposingStreamResultTests()
    {
    }

    [Fact]
    public async Task ExecuteAsync_WhenContentIsProvided_ShouldWriteEveryByteToResponseBody()
    {
        // Arrange
        byte[] expectedBytes = CreateBytes(512);
        using MemoryStream content = new(expectedBytes);
        DisposingStreamResult sut = new(content, "audio/mpeg", expectedBytes.Length);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await sut.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.Body.Position = 0;
        using MemoryStream writtenBody = new();
        await httpContext.Response.Body.CopyToAsync(writtenBody);
        Assert.Equal(expectedBytes, writtenBody.ToArray());
    }

    [Fact]
    public async Task ExecuteAsync_WhenContentLengthIsProvided_ShouldSetContentTypeAndContentLength()
    {
        // Arrange
        byte[] contentBytes = CreateBytes(128);
        using MemoryStream content = new(contentBytes);
        DisposingStreamResult sut = new(content, "audio/mp4", contentBytes.Length);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await sut.ExecuteAsync(httpContext);

        // Assert
        Assert.Equal("audio/mp4", httpContext.Response.ContentType);
        Assert.Equal(contentBytes.Length, httpContext.Response.ContentLength);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContentLengthIsNull_ShouldLeaveResponseContentLengthUnset()
    {
        // Arrange
        byte[] contentBytes = CreateBytes(64);
        using MemoryStream content = new(contentBytes);
        DisposingStreamResult sut = new(content, "audio/ogg", null);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await sut.ExecuteAsync(httpContext);

        // Assert
        Assert.Equal("audio/ogg", httpContext.Response.ContentType);
        Assert.Null(httpContext.Response.ContentLength);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWriteCompletes_ShouldDisposeTheContentStream()
    {
        // Arrange
        byte[] contentBytes = CreateBytes(32);
        DisposalTrackingStream content = new(contentBytes);
        DisposingStreamResult sut = new(content, "audio/mpeg", contentBytes.Length);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await sut.ExecuteAsync(httpContext);

        // Assert
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCopyToAsyncThrows_ShouldDisposeTheContentStreamAndPropagateTheException()
    {
        // Arrange
        ThrowingCopyStream content = new(CreateBytes(32));
        DisposingStreamResult sut = new(content, "audio/mpeg", null);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        Task operationTask = sut.ExecuteAsync(httpContext);

        // Assert
        await Assert.ThrowsAsync<IOException>(() => operationTask);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStreamWasPartiallyRead_ShouldCopyFromTheCurrentPosition()
    {
        // Arrange
        byte[] contentBytes = CreateBytes(100);
        using MemoryStream content = new(contentBytes);
        content.Position = 37;
        DisposingStreamResult sut = new(content, "audio/wav", contentBytes.Length - 37);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await sut.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.Body.Position = 0;
        using MemoryStream writtenBody = new();
        await httpContext.Response.Body.CopyToAsync(writtenBody);
        Assert.Equal(contentBytes[37..], writtenBody.ToArray());
    }

    /// <summary>
    /// Creates a <see cref="DefaultHttpContext"/> with a writable response body.
    /// </summary>
    /// <returns>The configured HTTP context.</returns>
    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    /// <summary>
    /// Creates a deterministic byte array that stands in for streamed content.
    /// </summary>
    /// <param name="length">The number of bytes to create.</param>
    /// <returns>The created bytes.</returns>
    private static byte[] CreateBytes(int length)
    {
        return [.. Enumerable.Range(0, length).Select(index => (byte)(index % 256))];
    }

    /// <summary>
    /// A memory stream that records whether it was disposed.
    /// </summary>
    private sealed class DisposalTrackingStream : MemoryStream
    {
        /// <summary>
        /// Gets whether the stream was disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposalTrackingStream"/> class.
        /// </summary>
        /// <param name="content">The bytes of the stream.</param>
        public DisposalTrackingStream(byte[] content) : base(content)
        {
        }

        /// <summary>
        /// Releases the resources of the stream and records the disposal.
        /// </summary>
        /// <param name="disposing">Whether managed resources are being disposed.</param>
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A memory stream whose copy operation always fails.
    /// </summary>
    private sealed class ThrowingCopyStream : MemoryStream
    {
        /// <summary>
        /// Gets whether the stream was disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingCopyStream"/> class.
        /// </summary>
        /// <param name="content">The bytes of the stream.</param>
        public ThrowingCopyStream(byte[] content) : base(content)
        {
        }

        /// <summary>
        /// Simulates a failed write to the destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        /// <param name="bufferSize">The size of the buffer used by the copy.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>This method never returns, it always throws.</returns>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            throw new IOException("The destination stream rejected the content.");
        }

        /// <summary>
        /// Releases the resources of the stream and records the disposal.
        /// </summary>
        /// <param name="disposing">Whether managed resources are being disposed.</param>
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
