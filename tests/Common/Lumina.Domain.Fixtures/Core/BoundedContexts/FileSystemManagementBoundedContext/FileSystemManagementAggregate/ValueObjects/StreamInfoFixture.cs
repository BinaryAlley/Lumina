#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="StreamInfo"/> value object.
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
    /// <param name="streamId">Optional. The unique identifier for the stream.</param>
    /// <param name="mimeType">Optional. The MIME type of the stream.</param>
    /// <param name="bitrate">Optional. The bitrate of the stream in kbps.</param>
    /// <param name="codec">Optional. The codec used for the stream.</param>
    /// <param name="resolution">Optional. The video resolution.</param>
    /// <param name="frameRate">Optional. The frame rate of the video stream.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio stream.</param>
    /// <param name="channels">Optional. The number of audio channels.</param>
    /// <returns>The created <see cref="StreamInfo"/>.</returns>
    public StreamInfo Create(
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

        Result<StreamInfo> streamInfoResult = StreamInfo.Create(
            streamId, mimeType, bitrate.Value, codec, resolution.Value, frameRate.Value, sampleRate.Value, channels.Value);

        if (streamInfoResult.IsFailure)
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
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
