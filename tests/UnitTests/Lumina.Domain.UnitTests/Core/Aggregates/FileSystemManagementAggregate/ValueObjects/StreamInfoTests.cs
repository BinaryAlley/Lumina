#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
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
        result.IsError.Should().BeFalse();
        result.Value.StreamId.Should().Be(streamId);
        result.Value.MimeType.Should().Be(mimeType);
        result.Value.Bitrate.Should().Be(bitrate);
        result.Value.Codec.Should().Be(codec);
        result.Value.Resolution.Should().Be(resolution);
        result.Value.FrameRate.Should().Be(frameRate);
        result.Value.SampleRate.Should().Be(sampleRate);
        result.Value.Channels.Should().Be(channels);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.StreamIdCannotBeEmpty);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.CodecCannotBeEmpty);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.BitrateMustBeAPositiveNumber);
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
        result.Should().BeTrue();
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
        result.Should().BeFalse();
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
        hashCode1.Should().Be(hashCode2);
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
        hashCode1.Should().NotBe(hashCode2);
    }
}
