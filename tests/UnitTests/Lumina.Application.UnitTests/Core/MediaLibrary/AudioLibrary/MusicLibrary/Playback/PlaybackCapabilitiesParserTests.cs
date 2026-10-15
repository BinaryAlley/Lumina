#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackCapabilitiesParser"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackCapabilitiesParserTests
{
    private readonly PlaybackCapabilityDtoFixture _playbackCapabilityDtoFixture = new();

    [Theory]
    [InlineData(null)] // an absent input falls back to the default capabilities
    [InlineData("")] // an empty input falls back to the default capabilities
    [InlineData("   ")] // a blank input falls back to the default capabilities
    [InlineData("not-a-pair")] // an input that does not name both a container and a codec falls back to the default capabilities
    [InlineData("a/b/c")] // an input with more than two segments falls back to the default capabilities
    [InlineData("a//b")] // an input whose container or codec is empty falls back to the default capabilities
    public void Parse_WhenInputIsNotAValidCapabilitySet_ShouldReturnDefaultCapabilities(string? input)
    {
        // Act
        IReadOnlyList<PlaybackCapabilityDto> capabilities = PlaybackCapabilitiesParser.Parse(input);

        // Assert
        Assert.Same(PlaybackCapabilitiesParser.Default, capabilities);
    }

    [Fact]
    public void Parse_WhenInputListsPairs_ShouldReturnThosePairs()
    {
        // Act
        IReadOnlyList<PlaybackCapabilityDto> capabilities = PlaybackCapabilitiesParser.Parse("mp3/mp3,flac/flac");

        // Assert
        Assert.NotSame(PlaybackCapabilitiesParser.Default, capabilities);
        Assert.Equal(2, capabilities.Count);
        Assert.Contains(_playbackCapabilityDtoFixture.Create(container: "mp3", codec: "mp3"), capabilities);
        Assert.Contains(_playbackCapabilityDtoFixture.Create(container: "flac", codec: "flac"), capabilities);
    }

    [Fact]
    public void Parse_WhenEntriesArePadded_ShouldTrimThem()
    {
        // Act
        IReadOnlyList<PlaybackCapabilityDto> capabilities = PlaybackCapabilitiesParser.Parse(" mp3 / mp3 , flac / flac ");

        // Assert
        Assert.Equal(2, capabilities.Count);
        Assert.Contains(_playbackCapabilityDtoFixture.Create(container: "mp3", codec: "mp3"), capabilities);
        Assert.Contains(_playbackCapabilityDtoFixture.Create(container: "flac", codec: "flac"), capabilities);
    }

    [Fact]
    public void Parse_WhenInputContainsInvalidEntriesAmongValidOnes_ShouldIgnoreTheInvalidOnes()
    {
        // Act
        IReadOnlyList<PlaybackCapabilityDto> capabilities = PlaybackCapabilitiesParser.Parse("mp3/mp3,,not-a-pair,/wav,wav/,wav/pcm");

        // Assert
        Assert.Equal(2, capabilities.Count);
        Assert.Contains(_playbackCapabilityDtoFixture.Create(container: "mp3", codec: "mp3"), capabilities);
        Assert.Contains(_playbackCapabilityDtoFixture.Create(container: "wav", codec: "pcm"), capabilities);
    }

    [Fact]
    public void Parse_WhenMorePairsThanTheMaximumAreListed_ShouldKeepOnlyTheFirstMaximum()
    {
        // Arrange
        List<string> pairs = [];
        for (int index = 0; index < PlaybackCapabilitiesParser.MAX_CAPABILITY_COUNT + 5; index++)
            pairs.Add($"container{index}/codec{index}");

        // Act
        IReadOnlyList<PlaybackCapabilityDto> capabilities = PlaybackCapabilitiesParser.Parse(string.Join(',', pairs));

        // Assert
        Assert.Equal(PlaybackCapabilitiesParser.MAX_CAPABILITY_COUNT, capabilities.Count);
    }
}
