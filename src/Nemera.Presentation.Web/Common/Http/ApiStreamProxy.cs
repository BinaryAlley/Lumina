#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Http;

/// <summary>
/// Writes a streamed API response to the browser, so the direct track stream and the normalized segment proxy share the same status, header, caching and body forwarding rules and cannot drift apart.
/// </summary>
public static class ApiStreamProxy
{
    /// <summary>
    /// Copies the streamed API response to the response sent to the browser, without buffering its content.
    /// </summary>
    /// <param name="response">The response sent to the browser.</param>
    /// <param name="streamResult">The streamed response of the API.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the copy when the client disconnects.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous copy operation.</returns>
    public static async Task WriteAsync(HttpResponse response, ApiStreamResult streamResult, CancellationToken cancellationToken)
    {
        response.StatusCode = (int)streamResult.StatusCode;
        if (!string.IsNullOrEmpty(streamResult.ContentType))
            response.ContentType = streamResult.ContentType;
        if (streamResult.ContentLength.HasValue)
            response.ContentLength = streamResult.ContentLength.Value;
        SetHeader(response, "Content-Range", streamResult.ContentRange);
        SetHeader(response, "Accept-Ranges", streamResult.AcceptRanges);
        SetHeader(response, "ETag", streamResult.ETag);
        SetHeader(response, "Last-Modified", streamResult.LastModified?.ToString("R"));
        if (streamResult.SegmentCount.HasValue)
            SetHeader(response, ApiStreamResult.SEGMENT_COUNT_HEADER_NAME, streamResult.SegmentCount.Value.ToString());
        SetHeader(response, ApiStreamResult.SEGMENT_START_SECONDS_HEADER_NAME, streamResult.SegmentStartSeconds);
        if (streamResult.TrackDurationSeconds.HasValue)
            SetHeader(response, ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME, streamResult.TrackDurationSeconds.Value.ToString());
        // The response is private to the cookie that authenticates this hop, and it never becomes publicly cacheable, whatever the API declared.
        response.Headers["Cache-Control"] = BuildCacheControl(streamResult.CacheControl);
        response.Headers["Vary"] = "Cookie";
        await streamResult.Content.CopyToAsync(response.Body, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets a response header to the given value, unless the value is missing.
    /// </summary>
    /// <param name="response">The response to set the header on.</param>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header, or <see langword="null"/> when the API did not send it.</param>
    private static void SetHeader(HttpResponse response, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            response.Headers[name] = value;
    }

    /// <summary>
    /// Builds the caching instructions of the browser response from the instructions of the API.
    /// </summary>
    /// <remarks>
    /// A streamed media response is specific to the authenticated user, so it must never be stored by a shared cache. Every other instruction of
    /// the API, like a maximum age for the private cache of the browser or a no-store, is kept, so the web hop never silently ignores a caching
    /// decision the API made for the resource.
    /// </remarks>
    /// <param name="apiCacheControl">The caching instructions of the API response, or <see langword="null"/> when it declared none.</param>
    /// <returns>The caching instructions of the response sent to the browser.</returns>
    private static string BuildCacheControl(string? apiCacheControl)
    {
        if (!CacheControlHeaderValue.TryParse(apiCacheControl, out CacheControlHeaderValue? cacheControl))
            cacheControl = new CacheControlHeaderValue();
        cacheControl.Public = false;
        cacheControl.Private = true;
        return cacheControl.ToString();
    }
}
