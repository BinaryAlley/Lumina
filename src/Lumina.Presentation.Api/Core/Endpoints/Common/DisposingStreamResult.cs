#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Common;

/// <summary>
/// A response result that writes an owned stream to the response without buffering it and releases the stream once the response is complete.
/// </summary>
/// <remarks>
/// The result owns the stream, so a lease protected resource that is served through it is always released, including when the client disconnects
/// before the whole content was written. Unlike a result built from bytes, it never materializes the content in memory.
/// </remarks>
public sealed class DisposingStreamResult : IResult
{
    private readonly Stream _content;
    private readonly string _contentType;
    private readonly long? _contentLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposingStreamResult"/> class.
    /// </summary>
    /// <param name="content">The stream to write, which this result owns and releases after the response is complete.</param>
    /// <param name="contentType">The media type of the content.</param>
    /// <param name="contentLength">The length of the content, in bytes, or <see langword="null"/> when it is not known in advance.</param>
    public DisposingStreamResult(Stream content, string contentType, long? contentLength)
    {
        _content = content;
        _contentType = contentType;
        _contentLength = contentLength;
    }

    /// <summary>
    /// Writes the content stream to the response.
    /// </summary>
    /// <param name="httpContext">The HTTP context of the request being answered.</param>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (_contentLength.HasValue)
            httpContext.Response.ContentLength = _contentLength.Value;
        httpContext.Response.ContentType = _contentType;
        await using (_content)
            await _content.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted).ConfigureAwait(false);
    }
}
