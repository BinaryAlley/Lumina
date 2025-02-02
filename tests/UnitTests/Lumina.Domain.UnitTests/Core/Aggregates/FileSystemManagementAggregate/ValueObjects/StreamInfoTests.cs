#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using System.Diagnostics.CodeAnalysis;
#endregion


namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="StreamInfo"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamInfoTests
{
    private readonly StreamInfoFixture _streamInfoFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamInfoTests"/> class.
    /// </summary>
    public StreamInfoTests()
    {
        _streamInfoFixture = new StreamInfoFixture();
    }

    [Fact]
    public void Create_WithValidInput_ShouldReturnStreamInfo()
    {
        // Arrange
        string streamId = "testStream";
        string mimeType = "video/mp4";
        int bitrate = 1000;
        string codec = "h264";
        Optional<string> resolution = Optional<string>.Some("1920x1080");
        Optional<float> frameRate = Optional<float>.Some(30.0f);
        Optional<int> sampleRate = Optional<int>.Some(44100);
        Optional<int> channels = Optional<int>.Some(2);

        // Act
        ErrorOr<StreamInfo> result = StreamInfo.Create(streamId, mimeType, bitrate, codec, resolution, frameRate, sampleRate, channels);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(streamId, result.Value.StreamId);
        Assert.Equal(mimeType, result.Value.MimeType);
        Assert.Equal(bitrate, result.Value.Bitrate);
        Assert.Equal(codec, result.Value.Codec);
        Assert.Equal(resolution, result.Value.Resolution);
        Assert.Equal(frameRate, result.Value.FrameRate);
        Assert.Equal(sampleRate, result.Value.SampleRate);
        Assert.Equal(channels, result.Value.Channels);
    }

    [Fact]
    public void Create_WithNullStreamId_ShouldReturnError()
    {
        // Arrange
        string? streamId = null;
        string mimeType = "video/mp4";
        int bitrate = 1000;
        string codec = "h264";

        // Act
        ErrorOr<StreamInfo> result = StreamInfo.Create(streamId!, mimeType, bitrate, codec, Optional<string>.None(), Optional<float>.None(), Optional<int>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.StreamIdCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WithNullCodec_ShouldReturnError()
    {
        // Arrange
        string streamId = "testStream";
        string mimeType = "video/mp4";
        int bitrate = 1000;
        string? codec = null;

        // Act
        ErrorOr<StreamInfo> result = StreamInfo.Create(streamId, mimeType, bitrate, codec!, Optional<string>.None(), Optional<float>.None(), Optional<int>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.CodecCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WithNonPositiveBitrate_ShouldReturnError()
    {
        // Arrange
        string streamId = "testStream";
        string mimeType = "video/mp4";
        int bitrate = 0;
        string codec = "h264";

        // Act
        ErrorOr<StreamInfo> result = StreamInfo.Create(streamId, mimeType, bitrate, codec, Optional<string>.None(), Optional<float>.None(), Optional<int>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.BitrateMustBeAPositiveNumber, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameProperties_ShouldReturnTrue()
    {
        // Arrange
        StreamInfo info1 = _streamInfoFixture.CreateStreamInfo();
        StreamInfo info2 = _streamInfoFixture.CreateStreamInfo(
            info1.StreamId, info1.MimeType, info1.Bitrate, info1.Codec,
            info1.Resolution, info1.FrameRate, info1.SampleRate, info1.Channels);

        // Act
        bool result = info1.Equals(info2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentProperties_ShouldReturnFalse()
    {
        // Arrange
        StreamInfo info1 = _streamInfoFixture.CreateStreamInfo();
        StreamInfo info2 = _streamInfoFixture.CreateStreamInfo();

        // Act
        bool result = info1.Equals(info2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WithSameProperties_ShouldReturnSameHashCode()
    {
        // Arrange
        StreamInfo info1 = _streamInfoFixture.CreateStreamInfo();
        StreamInfo info2 = _streamInfoFixture.CreateStreamInfo(
            info1.StreamId, info1.MimeType, info1.Bitrate, info1.Codec,
            info1.Resolution, info1.FrameRate, info1.SampleRate, info1.Channels);

        // Act
        int hashCode1 = info1.GetHashCode();
        int hashCode2 = info2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentProperties_ShouldReturnDifferentHashCode()
    {
        // Arrange
        StreamInfo info1 = _streamInfoFixture.CreateStreamInfo();
        StreamInfo info2 = _streamInfoFixture.CreateStreamInfo();

        // Act
        int hashCode1 = info1.GetHashCode();
        int hashCode2 = info2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }
}
