#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackFormatService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackFormatServiceTests
{
    private readonly PlaybackFormatService _sut;
    private readonly PlaybackSourceFormatDtoFixture _playbackSourceFormatDtoFixture = new();
    private readonly PlaybackCapabilityDtoFixture _playbackCapabilityDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackFormatServiceTests"/> class.
    /// </summary>
    public PlaybackFormatServiceTests()
    {
        _sut = new PlaybackFormatService();
    }

    [Fact]
    public void DecideDelivery_WhenPlayerAdvertisesTheStoredFormat_ShouldReturnDirectPlay()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "FLAC", codec: "FLAC");
        IReadOnlyList<PlaybackCapabilityDto> capabilities = [_playbackCapabilityDtoFixture.Create(container: "FLAC", codec: "FLAC")];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.DirectPlay, result.Value.Mode);
        Assert.Equal("FLAC", result.Value.Container);
        Assert.Equal("FLAC", result.Value.Codec);
        Assert.Equal(PlaybackDeliveryShape.ByteRangeSeekable, result.Value.Shape);
        Assert.Equal(0, result.Value.SegmentDurationSeconds);
    }

    [Fact]
    public void DecideDelivery_WhenPlayerDecodesTheCodecButNotTheContainer_ShouldReturnRemux()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "AIFF", codec: "PCM");
        IReadOnlyList<PlaybackCapabilityDto> capabilities =
        [
            _playbackCapabilityDtoFixture.Create(container: "WAV", codec: "PCM"),
            _playbackCapabilityDtoFixture.Create(container: "FLAC", codec: "FLAC")
        ];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Remux, result.Value.Mode);
        Assert.Equal("WAV", result.Value.Container);
        Assert.Equal("PCM", result.Value.Codec);
        Assert.Equal(PlaybackDeliveryShape.TimeAddressedSegments, result.Value.Shape);
        Assert.Equal(6, result.Value.SegmentDurationSeconds);
    }

    [Fact]
    public void DecideDelivery_WhenPlayerCannotDecodeTheStoredCodec_ShouldReturnTranscode()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "WMA", codec: "WMA");
        IReadOnlyList<PlaybackCapabilityDto> capabilities = [_playbackCapabilityDtoFixture.Create(container: "MP3", codec: "MP3")];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Transcode, result.Value.Mode);
        Assert.Equal("MP3", result.Value.Container);
        Assert.Equal("MP3", result.Value.Codec);
        Assert.Equal(PlaybackDeliveryShape.TimeAddressedSegments, result.Value.Shape);
        Assert.Equal(6, result.Value.SegmentDurationSeconds);
    }

    [Fact]
    public void DecideDelivery_WhenTheDefaultCapabilitiesAreUsed_ShouldReturnDirectPlay()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "MP3", codec: "MP3");

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, PlaybackCapabilitiesParser.Default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.DirectPlay, result.Value.Mode);
    }

    [Fact]
    public void DecideDelivery_WhenDefaultCapabilitiesAreUsedForAnAacTrackInMp4_ShouldReturnRemuxToRawAac()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "MP4", codec: "AAC");

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, PlaybackCapabilitiesParser.Default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Remux, result.Value.Mode);
        Assert.Equal("AAC", result.Value.Container);
        Assert.Equal("AAC", result.Value.Codec);
    }

    [Theory]
    [InlineData("MP3", "MPEG-1 Layer 3", "MP3")] // the free form description of an MPEG-1 Layer 3 stream
    [InlineData("MP3", "MP3", "MP3")] // a codec that is already named canonically
    [InlineData("FLAC", "Flac", "FLAC")] // the description of a FLAC stream
    [InlineData("OGG", "Vorbis I", "VORBIS")] // the description of a Vorbis stream
    [InlineData("OGG", "Opus", "OPUS")] // the description of an Opus stream
    [InlineData("MP4", "Apple Lossless", "ALAC")] // the description of an Apple Lossless stream
    [InlineData("AAC", "MPEG-4 AAC", "AAC")] // a description naming both MPEG and AAC is AAC, not MP3
    [InlineData("WAV", "PCM", "PCM")] // the description of an uncompressed stream
    [InlineData("WAV", "Uncompressed", "PCM")] // another description of an uncompressed stream
    [InlineData("WMA", "Windows Media Audio", "WMA")] // the description of a Windows Media stream
    public void DecideDelivery_WhenTheStoredCodecDescriptionIsRecognizable_ShouldCanonicalizeItAndDirectPlay(string container, string codecDescription, string canonicalCodec)
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: container, codec: codecDescription);
        IReadOnlyList<PlaybackCapabilityDto> capabilities = [_playbackCapabilityDtoFixture.Create(container: container, codec: canonicalCodec)];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.DirectPlay, result.Value.Mode);
        Assert.Equal(canonicalCodec, result.Value.Codec);
    }

    [Theory]
    [InlineData("MPEG-1 Layer 2")] // MP2 shares the MP3 container but an MP3 decoder cannot decode it
    [InlineData("MPEG-1 Layer 1")] // MP1 shares the MP3 container but an MP3 decoder cannot decode it
    [InlineData("MPEG-4 Audio")] // a description that names no decodable codec is not mistaken for MP3
    public void DecideDelivery_WhenAnMpegDescriptionIsNotLayer3_ShouldReturnTranscode(string codecDescription)
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "MP3", codec: codecDescription);
        IReadOnlyList<PlaybackCapabilityDto> capabilities = [_playbackCapabilityDtoFixture.Create(container: "MP3", codec: "MP3")];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Transcode, result.Value.Mode);
    }

    [Theory]
    [InlineData(null, null)] // an absent probe result names no codec
    [InlineData("", "")] // an empty probe result names no codec
    [InlineData("MP3", "AC-3")] // a codec description the server does not recognize
    public void DecideDelivery_WhenTheStoredFormatIsUnknown_ShouldReturnTranscode(string? container, string? codec)
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(
            container: container,
            codec: codec,
            includeContainer: container is not null,
            includeCodec: codec is not null);
        IReadOnlyList<PlaybackCapabilityDto> capabilities = [_playbackCapabilityDtoFixture.Create(container: "MP3", codec: "MP3")];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Transcode, result.Value.Mode);
    }

    [Fact]
    public void DecideDelivery_WhenSeveralProducibleCodecsAreAdvertised_ShouldTranscodeToTheMostPreferredOne()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "OGG", codec: "OPUS");
        IReadOnlyList<PlaybackCapabilityDto> capabilities =
        [
            _playbackCapabilityDtoFixture.Create(container: "FLAC", codec: "FLAC"),
            _playbackCapabilityDtoFixture.Create(container: "AAC", codec: "AAC")
        ];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Transcode, result.Value.Mode);
        Assert.Equal("AAC", result.Value.Codec);
    }

    [Fact]
    public void DecideDelivery_WhenSeveralContainersAreAdvertisedForTheSameCodec_ShouldRemuxToThePreferredContainer()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "MATROSKA", codec: "AAC");
        IReadOnlyList<PlaybackCapabilityDto> capabilities =
        [
            _playbackCapabilityDtoFixture.Create(container: "AAC", codec: "AAC"),
            _playbackCapabilityDtoFixture.Create(container: "MP4", codec: "AAC")
        ];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Remux, result.Value.Mode);
        Assert.Equal("MP4", result.Value.Container);
        Assert.Equal("AAC", result.Value.Codec);
    }

    [Fact]
    public void DecideDelivery_WhenThePreferredContainerForTheCodecIsNotAdvertised_ShouldRemuxToTheOtherProducibleContainer()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "MATROSKA", codec: "AAC");
        IReadOnlyList<PlaybackCapabilityDto> capabilities = [_playbackCapabilityDtoFixture.Create(container: "AAC", codec: "AAC")];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Remux, result.Value.Mode);
        Assert.Equal("AAC", result.Value.Container);
        Assert.Equal("AAC", result.Value.Codec);
    }

    [Fact]
    public void DecideDelivery_WhenThePlayerNamesAContainerTheServerDoesNotProduce_ShouldDeliverTheServerContainer()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "AIFF", codec: "PCM");
        IReadOnlyList<PlaybackCapabilityDto> capabilities =
        [
            _playbackCapabilityDtoFixture.Create(container: "evil;$(x)", codec: "PCM"),
            _playbackCapabilityDtoFixture.Create(container: "wav", codec: "pcm")
        ];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PlaybackDeliveryMode.Remux, result.Value.Mode);
        Assert.Equal("WAV", result.Value.Container);
        Assert.Equal("PCM", result.Value.Codec);
    }

    [Fact]
    public void DecideDelivery_WhenThePlayerOnlyNamesContainersTheServerDoesNotProduce_ShouldReturnNoSupportedPlaybackFormat()
    {
        // Arrange
        PlaybackSourceFormatDto sourceFormat = _playbackSourceFormatDtoFixture.Create(container: "AIFF", codec: "PCM");
        IReadOnlyList<PlaybackCapabilityDto> capabilities = [_playbackCapabilityDtoFixture.Create(container: "evil;$(x)", codec: "PCM")];

        // Act
        Result<PlaybackDeliveryDecisionDto> result = _sut.DecideDelivery(sourceFormat, capabilities);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playback.NoSupportedPlaybackFormat, result.FirstError);
    }
}
