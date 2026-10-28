#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Api;

/// <summary>
/// Contains unit tests for the <see cref="ApiStreamResult"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApiStreamResultTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiStreamResultTests"/> class.
    /// </summary>
    public ApiStreamResultTests()
    {
    }

    [Fact]
    public async Task CreateAsync_WhenResponseDeclaresHeaders_ShouldExposeTheStatusAndTheContentHeaders()
    {
        // Arrange
        byte[] contentBytes = CreateBytes(64);
        using HttpResponseMessage response = CreateResponse(contentBytes);
        response.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, contentBytes.Length - 1, 1024);
        response.Headers.AcceptRanges.Add("bytes");
        response.Headers.ETag = new EntityTagHeaderValue("\"etag-value\"");
        DateTimeOffset lastModified = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        response.Content.Headers.LastModified = lastModified;
        response.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60) };

        // Act
        await using ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, result.StatusCode);
        Assert.Equal("audio/mp4", result.ContentType);
        Assert.Equal(contentBytes.Length, result.ContentLength);
        Assert.Equal("bytes 0-63/1024", result.ContentRange);
        Assert.Equal("bytes", result.AcceptRanges);
        Assert.Equal("\"etag-value\"", result.ETag);
        Assert.Equal(lastModified, result.LastModified);
        Assert.Contains("max-age=60", result.CacheControl);
    }

    [Theory]
    [InlineData("0")] // an empty production reports zero segments
    [InlineData("1")] // a single segment production
    [InlineData("42")] // a multi segment production
    public async Task CreateAsync_WhenSegmentCountIsValid_ShouldExposeIt(string segmentCount)
    {
        // Arrange
        using HttpResponseMessage response = CreateResponse(CreateBytes(8));
        response.Headers.TryAddWithoutValidation(ApiStreamResult.SEGMENT_COUNT_HEADER_NAME, segmentCount);

        // Act
        await using ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Assert
        Assert.Equal(int.Parse(segmentCount), result.SegmentCount);
    }

    [Theory]
    [InlineData(null)] // the header is absent
    [InlineData("")] // the header is empty
    [InlineData("not-a-number")] // the header is malformed
    [InlineData("-1")] // a negative count is treated as absent
    [InlineData("1.5")] // a fractional count is treated as absent
    public async Task CreateAsync_WhenSegmentCountIsInvalid_ShouldReturnNull(string? segmentCount)
    {
        // Arrange
        using HttpResponseMessage response = CreateResponse(CreateBytes(8));
        if (segmentCount is not null)
            response.Headers.TryAddWithoutValidation(ApiStreamResult.SEGMENT_COUNT_HEADER_NAME, segmentCount);

        // Act
        await using ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Assert
        Assert.Null(result.SegmentCount);
    }

    [Theory]
    [InlineData("0")] // the first segment starts at the beginning of the track
    [InlineData("6.5")] // a decimal with a dot is forwarded as the API wrote it
    [InlineData("6,5")] // a decimal with a comma, which is not the invariant separator, is still forwarded verbatim
    public async Task CreateAsync_WhenSegmentStartIsPresent_ShouldForwardItVerbatim(string segmentStart)
    {
        // Arrange
        using HttpResponseMessage response = CreateResponse(CreateBytes(8));
        response.Headers.TryAddWithoutValidation(ApiStreamResult.SEGMENT_START_SECONDS_HEADER_NAME, segmentStart);

        // Act
        await using ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Assert
        Assert.Equal(segmentStart, result.SegmentStartSeconds);
    }

    [Fact]
    public async Task CreateAsync_WhenSegmentStartIsAbsent_ShouldReturnNull()
    {
        // Arrange
        using HttpResponseMessage response = CreateResponse(CreateBytes(8));

        // Act
        await using ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Assert
        Assert.Null(result.SegmentStartSeconds);
    }

    [Theory]
    [InlineData("1")] // a one second production
    [InlineData("300")] // a five minute production
    public async Task CreateAsync_WhenTrackDurationIsPositive_ShouldExposeIt(string trackDuration)
    {
        // Arrange
        using HttpResponseMessage response = CreateResponse(CreateBytes(8));
        response.Headers.TryAddWithoutValidation(ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME, trackDuration);

        // Act
        await using ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Assert
        Assert.Equal(int.Parse(trackDuration), result.TrackDurationSeconds);
    }

    [Theory]
    [InlineData(null)] // the header is absent
    [InlineData("0")] // a zero duration is treated as absent
    [InlineData("-5")] // a negative duration is treated as absent
    [InlineData("not-a-number")] // the header is malformed
    public async Task CreateAsync_WhenTrackDurationIsInvalid_ShouldReturnNull(string? trackDuration)
    {
        // Arrange
        using HttpResponseMessage response = CreateResponse(CreateBytes(8));
        if (trackDuration is not null)
            response.Headers.TryAddWithoutValidation(ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME, trackDuration);

        // Act
        await using ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Assert
        Assert.Null(result.TrackDurationSeconds);
    }

    [Fact]
    public async Task DisposeAsync_WhenCalled_ShouldDisposeTheContentStreamAndTheResponse()
    {
        // Arrange
        HttpResponseMessage response = CreateResponse(CreateBytes(16));
        ApiStreamResult result = await ApiStreamResult.CreateAsync(response, CancellationToken.None);

        // Act
        await result.DisposeAsync();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => result.Content.ReadByte());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsByteArrayAsync());
    }

    /// <summary>
    /// Creates a streamed API response with a media type and a partial content status.
    /// </summary>
    /// <param name="contentBytes">The bytes of the content of the response.</param>
    /// <returns>The created HTTP response.</returns>
    private static HttpResponseMessage CreateResponse(byte[] contentBytes)
    {
        HttpResponseMessage response = new(HttpStatusCode.PartialContent)
        {
            Content = new ByteArrayContent(contentBytes)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp4");
        return response;
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
