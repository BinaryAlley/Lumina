#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Api;

/// <summary>
/// <see cref="HttpContent"/> test double that exposes whether the client buffered the content before handing it to the caller,
/// whether the content was disposed, and whether obtaining its stream is allowed to fail.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TrackingStreamContent : HttpContent
{
    private readonly byte[] _contentBytes;
    private readonly MemoryStream _contentStream;
    private readonly bool _shouldFailStreamCreation;

    /// <summary>
    /// Gets a value indicating whether the content was buffered by the client before it was handed to the caller.
    /// </summary>
    public bool WasBufferedByClient { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the content was disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the stream the content hands over when it is read without being buffered first.
    /// </summary>
    public Stream ContentStream => _contentStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackingStreamContent"/> class.
    /// </summary>
    /// <param name="contentBytes">The bytes of the content.</param>
    /// <param name="mediaType">The media type the content declares.</param>
    /// <param name="shouldFailStreamCreation">Whether obtaining the content stream should fail.</param>
    public TrackingStreamContent(byte[] contentBytes, string mediaType = "application/octet-stream", bool shouldFailStreamCreation = false)
    {
        _contentBytes = contentBytes;
        _contentStream = new MemoryStream(contentBytes);
        _shouldFailStreamCreation = shouldFailStreamCreation;
        Headers.ContentType = new MediaTypeHeaderValue(mediaType);
    }

    /// <summary>
    /// Writes the content to the provided stream, recording that the client buffered it.
    /// </summary>
    /// <param name="stream">The stream the content is written to.</param>
    /// <param name="context">The transport context of the serialization, if any.</param>
    /// <returns>A task that represents the asynchronous serialization.</returns>
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        WasBufferedByClient = true;
        return stream.WriteAsync(_contentBytes, 0, _contentBytes.Length);
    }

    /// <summary>
    /// Hands over the known content stream without buffering the content first.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The content stream.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the content is configured to fail obtaining its stream.</exception>
    protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
    {
        if (_shouldFailStreamCreation)
            throw new InvalidOperationException("The content stream cannot be obtained.");
        return Task.FromResult<Stream>(_contentStream);
    }

    /// <summary>
    /// Computes the length of the content.
    /// </summary>
    /// <param name="length">The length of the content in bytes.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    protected override bool TryComputeLength(out long length)
    {
        length = _contentBytes.Length;
        return true;
    }

    /// <summary>
    /// Disposes the content, recording that it was disposed.
    /// </summary>
    /// <param name="disposing">Whether the content is being disposed.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            IsDisposed = true;
        base.Dispose(disposing);
    }
}
