#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Http;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Http;

/// <summary>
/// Contains unit tests for the <see cref="ApiStreamProxy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApiStreamProxyTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiStreamProxyTests"/> class.
    /// </summary>
    public ApiStreamProxyTests()
    {
    }

    [Fact]
    public async Task WriteAsync_WhenCalled_ShouldCopyTheStatusCodeAndTheContentHeaders()
    {
        // Arrange
        byte[] contentBytes = CreateBytes(96);
        using HttpResponseMessage apiResponse = CreateResponse(HttpStatusCode.PartialContent, contentBytes);
        apiResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp4");
        apiResponse.Headers.AcceptRanges.Add("bytes");
        apiResponse.Headers.ETag = new EntityTagHeaderValue("\"etag-value\"");
        DateTimeOffset lastModified = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        apiResponse.Content.Headers.LastModified = lastModified;
        apiResponse.Content.Headers.ContentRange = new ContentRangeHeaderValue(10, 105, 1024);
        apiResponse.Headers.TryAddWithoutValidation(ApiStreamResult.SEGMENT_COUNT_HEADER_NAME, "9");
        apiResponse.Headers.TryAddWithoutValidation(ApiStreamResult.SEGMENT_START_SECONDS_HEADER_NAME, "6.5");
        apiResponse.Headers.TryAddWithoutValidation(ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME, "300");
        await using ApiStreamResult streamResult = await ApiStreamResult.CreateAsync(apiResponse, CancellationToken.None);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await ApiStreamProxy.WriteAsync(httpContext.Response, streamResult, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status206PartialContent, httpContext.Response.StatusCode);
        Assert.Equal("audio/mp4", httpContext.Response.ContentType);
        Assert.Equal(contentBytes.Length, httpContext.Response.ContentLength);
        Assert.Equal("bytes 10-105/1024", httpContext.Response.Headers["Content-Range"].ToString());
        Assert.Equal("bytes", httpContext.Response.Headers["Accept-Ranges"].ToString());
        Assert.Equal("\"etag-value\"", httpContext.Response.Headers["ETag"].ToString());
        Assert.Equal(lastModified.ToString("R"), httpContext.Response.Headers["Last-Modified"].ToString());
        Assert.Equal("9", httpContext.Response.Headers[ApiStreamResult.SEGMENT_COUNT_HEADER_NAME].ToString());
        Assert.Equal("6.5", httpContext.Response.Headers[ApiStreamResult.SEGMENT_START_SECONDS_HEADER_NAME].ToString());
        Assert.Equal("300", httpContext.Response.Headers[ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME].ToString());
        Assert.Equal("Cookie", httpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task WriteAsync_WhenOptionalHeadersAreAbsent_ShouldNotAddThem()
    {
        // Arrange
        using HttpResponseMessage apiResponse = CreateResponse(HttpStatusCode.OK, CreateBytes(16));
        await using ApiStreamResult streamResult = await ApiStreamResult.CreateAsync(apiResponse, CancellationToken.None);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await ApiStreamProxy.WriteAsync(httpContext.Response, streamResult, CancellationToken.None);

        // Assert
        Assert.False(httpContext.Response.Headers.ContainsKey("Content-Range"));
        Assert.False(httpContext.Response.Headers.ContainsKey("Accept-Ranges"));
        Assert.False(httpContext.Response.Headers.ContainsKey("ETag"));
        Assert.False(httpContext.Response.Headers.ContainsKey("Last-Modified"));
        Assert.False(httpContext.Response.Headers.ContainsKey(ApiStreamResult.SEGMENT_COUNT_HEADER_NAME));
        Assert.False(httpContext.Response.Headers.ContainsKey(ApiStreamResult.SEGMENT_START_SECONDS_HEADER_NAME));
        Assert.False(httpContext.Response.Headers.ContainsKey(ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME));
    }

    [Theory]
    [InlineData("public, max-age=60", new[] { "private", "max-age=60" })] // the public instruction is dropped and the max age is preserved
    [InlineData("no-store", new[] { "private", "no-store" })] // the no store instruction is preserved and privacy is added
    [InlineData(null, new[] { "private" })] // an absent API instruction becomes a private instruction
    public async Task WriteAsync_WhenApiDeclaresCaching_ShouldAlwaysProduceAPrivateInstruction(string? apiCacheControl, string[] expectedFragments)
    {
        // Arrange
        using HttpResponseMessage apiResponse = CreateResponse(HttpStatusCode.OK, CreateBytes(16));
        if (apiCacheControl is not null)
            apiResponse.Headers.CacheControl = CacheControlHeaderValue.Parse(apiCacheControl);
        await using ApiStreamResult streamResult = await ApiStreamResult.CreateAsync(apiResponse, CancellationToken.None);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await ApiStreamProxy.WriteAsync(httpContext.Response, streamResult, CancellationToken.None);

        // Assert
        string cacheControl = httpContext.Response.Headers["Cache-Control"].ToString();
        Assert.DoesNotContain("public", cacheControl, StringComparison.OrdinalIgnoreCase);
        foreach (string fragment in expectedFragments)
            Assert.Contains(fragment, cacheControl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WriteAsync_WhenCalled_ShouldCopyTheBodyExactly()
    {
        // Arrange
        byte[] contentBytes = CreateBytes(2048);
        using HttpResponseMessage apiResponse = CreateResponse(HttpStatusCode.OK, contentBytes);
        await using ApiStreamResult streamResult = await ApiStreamResult.CreateAsync(apiResponse, CancellationToken.None);
        DefaultHttpContext httpContext = CreateHttpContext();

        // Act
        await ApiStreamProxy.WriteAsync(httpContext.Response, streamResult, CancellationToken.None);

        // Assert
        httpContext.Response.Body.Position = 0;
        using MemoryStream writtenBody = new();
        httpContext.Response.Body.CopyTo(writtenBody);
        Assert.Equal(contentBytes, writtenBody.ToArray());
    }

    [Fact]
    public async Task WriteAsync_WhenCancellationIsRequested_ShouldCancelTheCopy()
    {
        // Arrange
        using HttpResponseMessage apiResponse = CreateResponse(HttpStatusCode.OK, CreateBytes(2048));
        await using ApiStreamResult streamResult = await ApiStreamResult.CreateAsync(apiResponse, CancellationToken.None);
        DefaultHttpContext httpContext = CreateHttpContext();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task operationTask = ApiStreamProxy.WriteAsync(httpContext.Response, streamResult, cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
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
    /// Creates a streamed API response with a status code and a body.
    /// </summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="contentBytes">The bytes of the content of the response.</param>
    /// <returns>The created HTTP response.</returns>
    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, byte[] contentBytes)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new ByteArrayContent(contentBytes)
        };
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
}
