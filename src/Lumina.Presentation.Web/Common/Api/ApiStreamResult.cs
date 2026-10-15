#region ========================================================================= USING =====================================================================================
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Api;

/// <summary>
/// Represents the streamed response of a remote API request. It exposes the status code, the range related headers and the
/// content stream of the response, so that a caller can forward the response to a client without ever buffering its content.
/// </summary>
public sealed class ApiStreamResult : IAsyncDisposable
{
    private readonly HttpResponseMessage _response;

    /// <summary>
    /// Gets the status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the content stream of the response.
    /// </summary>
    public Stream Content { get; }

    /// <summary>
    /// Gets the media type of the content of the response, or <see langword="null"/> when the response declares none.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the length of the content of the response, or <see langword="null"/> when the response declares none.
    /// </summary>
    public long? ContentLength { get; }

    /// <summary>
    /// Gets the range of the content of the response, when the response is a partial content response.
    /// </summary>
    public string? ContentRange { get; }

    /// <summary>
    /// Gets the ranges the remote API accepts, when the response declares them.
    /// </summary>
    public string? AcceptRanges { get; }

    /// <summary>
    /// Gets the entity tag of the content of the response, when the response declares one.
    /// </summary>
    public string? ETag { get; }

    /// <summary>
    /// Gets the date and time the content of the response was last modified, when the response declares it.
    /// </summary>
    public DateTimeOffset? LastModified { get; }

    /// <summary>
    /// Gets the caching instructions of the response, when the response declares them.
    /// </summary>
    public string? CacheControl { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiStreamResult"/> class.
    /// </summary>
    /// <param name="response">The response returned by the remote API.</param>
    /// <param name="content">The content stream of the response.</param>
    private ApiStreamResult(HttpResponseMessage response, Stream content)
    {
        _response = response;
        Content = content;
        StatusCode = response.StatusCode;
        ContentType = response.Content.Headers.ContentType?.ToString();
        ContentLength = response.Content.Headers.ContentLength;
        ContentRange = response.Content.Headers.ContentRange?.ToString();
        AcceptRanges = response.Headers.AcceptRanges.Count > 0 ? string.Join(", ", response.Headers.AcceptRanges) : null;
        ETag = response.Headers.ETag?.ToString();
        LastModified = response.Content.Headers.LastModified;
        CacheControl = response.Headers.CacheControl?.ToString();
    }

    /// <summary>
    /// Creates a <see cref="ApiStreamResult"/> from a response, without buffering the content of the response.
    /// </summary>
    /// <param name="response">The response returned by the remote API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The streamed response.</returns>
    public static async Task<ApiStreamResult> CreateAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        Stream content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return new ApiStreamResult(response, content);
    }

    /// <summary>
    /// Disposes the content stream and the response it belongs to.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync().ConfigureAwait(false);
        _response.Dispose();
    }
}
