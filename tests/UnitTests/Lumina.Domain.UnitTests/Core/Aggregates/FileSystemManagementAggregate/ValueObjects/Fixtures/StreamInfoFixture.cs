#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;

/// <summary>
/// Fixture class for the <see cref="StreamInfo"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamInfoFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamInfoFixture"/> class.
    /// </summary>
    public StreamInfoFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="StreamInfo"/>.
    /// </summary>
    /// <param name="streamId">Optional. The unique identifier for the stream. If not provided, a default value may be used.</param>
    /// <param name="mimeType">Optional. The MIME type of the stream (e.g., video/mp4, audio/mpeg). If not provided, a default MIME type may be used.</param>
    /// <param name="bitrate">Optional. The bitrate of the stream in kbps. If not provided, a default value may be used.</param>
    /// <param name="codec">Optional. The codec used for the stream (e.g., H.264, AAC). If not provided, a default codec may be used.</param>
    /// <param name="resolution">Optional. The video resolution (e.g., 1920x1080). If not provided, a default resolution may be used.</param>
    /// <param name="frameRate">Optional. The frame rate of the video stream in frames per second (fps). If not provided, a default frame rate may be used.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio stream in Hz. If not provided, a default sample rate may be used.</param>
    /// <param name="channels">Optional. The number of audio channels (e.g., 2 for stereo, 6 for 5.1 surround). If not provided, a default channel count may be used.</param>
    /// <returns>The created <see cref="StreamInfo"/>.</returns>
    public StreamInfo CreateStreamInfo(
        string? streamId = null,
        string? mimeType = null,
        int? bitrate = null,
        string? codec = null,
        Optional<string>? resolution = null,
        Optional<float>? frameRate = null,
        Optional<int>? sampleRate = null,
        Optional<int>? channels = null)
    {
        streamId ??= _faker.Random.AlphaNumeric(10);
        mimeType ??= _faker.System.MimeType();
        bitrate ??= _faker.Random.Number(1, 10000);
        codec ??= _faker.Random.Word();
        resolution ??= Optional<string>.FromNullable(_faker.Random.Bool() ? $"{_faker.Random.Number(100, 4000)}x{_faker.Random.Number(100, 4000)}" : null);
        frameRate ??= Optional<float>.FromNullable(_faker.Random.Bool() ? (float?)_faker.Random.Float(1, 120) : null);
        sampleRate ??= Optional<int>.FromNullable(_faker.Random.Bool() ? (int?)_faker.Random.Number(8000, 192000) : null);
        channels ??= Optional<int>.FromNullable(_faker.Random.Bool() ? (int?)_faker.Random.Number(1, 8) : null);

        ErrorOr<StreamInfo> streamInfoResult = StreamInfo.Create(
            streamId, mimeType, bitrate.Value, codec, resolution.Value, frameRate.Value, sampleRate.Value, channels.Value);

        if (streamInfoResult.IsError)
            throw new InvalidOperationException("Failed to create StreamInfo: " + string.Join(", ", streamInfoResult.Errors));
        return streamInfoResult.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="StreamInfo"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<StreamInfo> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => CreateStreamInfo()).ToList();
    }
}
