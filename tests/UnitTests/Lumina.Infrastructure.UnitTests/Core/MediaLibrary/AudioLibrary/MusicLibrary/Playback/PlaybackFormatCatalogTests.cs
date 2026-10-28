#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackFormatCatalog"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackFormatCatalogTests
{
    [Fact]
    public void Formats_WhenRead_ShouldExposeTheDocumentedCanonicalContainerAndCodecPairsInPreferenceOrder()
    {
        // Arrange
        (string Container, string Codec)[] expectedPairs =
        [
            ("MP4", "AAC"), // the preferred container of the preferred codec
            ("AAC", "AAC"), // the second container of the preferred codec
            ("OGG", "OPUS"), // the preferred lossy codec after AAC
            ("MP3", "MP3"), // the compatibility codec
            ("OGG", "VORBIS"), // the second lossy codec in OGG
            ("FLAC", "FLAC"), // the lossless codec
            ("WAV", "PCM") // the uncompressed codec
        ];

        // Act
        IReadOnlyList<NormalizedAudioFormatDto> result = PlaybackFormatCatalog.Formats;

        // Assert
        Assert.Equal(expectedPairs, result.Select(format => (format.Container, format.Codec)));
    }

    [Fact]
    public void TranscodeCodecPreference_WhenRead_ShouldListEachCodecOnceInOrderOfFirstAppearanceInFormats()
    {
        // Arrange
        List<string> expectedCodecs = [];
        foreach (NormalizedAudioFormatDto format in PlaybackFormatCatalog.Formats)
            if (!expectedCodecs.Contains(format.Codec, StringComparer.OrdinalIgnoreCase))
                expectedCodecs.Add(format.Codec);

        // Act
        IReadOnlyList<string> result = PlaybackFormatCatalog.TranscodeCodecPreference;

        // Assert
        Assert.Equal(expectedCodecs, result);
        Assert.Equal(["AAC", "OPUS", "MP3", "VORBIS", "FLAC", "PCM"], result);
        Assert.Equal(result.Count, result.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void ProducibleContainersByCodec_WhenRead_ShouldGroupTheContainersPerCodecInCatalogOrder()
    {
        // Arrange
        Dictionary<string, List<string>> expectedContainersByCodec = new(StringComparer.OrdinalIgnoreCase);
        foreach (NormalizedAudioFormatDto format in PlaybackFormatCatalog.Formats)
        {
            if (!expectedContainersByCodec.TryGetValue(format.Codec, out List<string>? containers))
            {
                containers = [];
                expectedContainersByCodec[format.Codec] = containers;
            }
            containers.Add(format.Container);
        }

        // Act
        IReadOnlyDictionary<string, IReadOnlyList<string>> result = PlaybackFormatCatalog.ProducibleContainersByCodec;

        // Assert
        Assert.Equal(expectedContainersByCodec.Count, result.Count);
        foreach (KeyValuePair<string, List<string>> expectedEntry in expectedContainersByCodec)
        {
            Assert.True(result.TryGetValue(expectedEntry.Key, out IReadOnlyList<string>? containers));
            Assert.Equal(expectedEntry.Value, containers);
        }
        Assert.Equal(["MP4", "AAC"], result["AAC"]); // the preferred codec has two producible containers, in preference order
        Assert.Equal(["OGG"], result["OPUS"]);
        Assert.Equal(["OGG"], result["VORBIS"]);
        Assert.Equal(["MP3"], result["MP3"]);
        Assert.Equal(["FLAC"], result["FLAC"]);
        Assert.Equal(["WAV"], result["PCM"]);
    }

    [Fact]
    public void ProducibleContainersByCodec_WhenRead_ShouldNotRepeatAContainerWithinACodec()
    {
        // Act
        IReadOnlyDictionary<string, IReadOnlyList<string>> result = PlaybackFormatCatalog.ProducibleContainersByCodec;

        // Assert
        foreach (IReadOnlyList<string> containers in result.Values)
            Assert.Equal(containers.Count, containers.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void ProducibleContainersByCodec_WhenRead_ShouldOnlyContainPairsThatExistInTheCatalog()
    {
        // Act
        IReadOnlyDictionary<string, IReadOnlyList<string>> result = PlaybackFormatCatalog.ProducibleContainersByCodec;

        // Assert
        foreach (KeyValuePair<string, IReadOnlyList<string>> entry in result)
            foreach (string container in entry.Value)
                Assert.Contains(PlaybackFormatCatalog.Formats, format =>
                    string.Equals(format.Codec, entry.Key, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(format.Container, container, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FormatsByPair_WhenRead_ShouldContainExactlyOneEntryPerCatalogFormat()
    {
        // Act
        IReadOnlyDictionary<string, NormalizedAudioFormatDto> result = PlaybackFormatCatalog.FormatsByPair;

        // Assert
        Assert.Equal(PlaybackFormatCatalog.Formats.Count, result.Count);
        Assert.All(PlaybackFormatCatalog.Formats, format =>
        {
            string pairKey = PlaybackFormatCatalog.BuildPairKey(format.Container, format.Codec);
            Assert.True(result.TryGetValue(pairKey, out NormalizedAudioFormatDto? indexedFormat));
            Assert.Equal(format, indexedFormat);
        });
    }

    [Theory]
    [InlineData("mp4/aac", "MP4", "AAC")] // the canonical pair in lower case
    [InlineData("Mp4/Aac", "MP4", "AAC")] // the canonical pair in mixed case
    [InlineData("aac/aac", "AAC", "AAC")] // the second format in the catalog, in lower case
    public void FormatsByPair_WhenLookedUpWithDifferentCasing_ShouldReturnTheCatalogFormat(string pairKey, string expectedContainer, string expectedCodec)
    {
        // Act
        bool found = PlaybackFormatCatalog.FormatsByPair.TryGetValue(pairKey, out NormalizedAudioFormatDto? result);

        // Assert
        Assert.True(found);
        Assert.NotNull(result);
        Assert.Equal(expectedContainer, result.Container);
        Assert.Equal(expectedCodec, result.Codec);
    }

    [Fact]
    public void BuildPairKey_WhenTheInputsArePaddedAndDifferentlyCased_ShouldTrimAndUppercaseThem()
    {
        // Act
        string result = PlaybackFormatCatalog.BuildPairKey("  mp4  ", "\taAc ");

        // Assert
        Assert.Equal("MP4/AAC", result);
    }

    [Fact]
    public void BuildPairKey_WhenTheInputsDifferOnlyInCasingAndPadding_ShouldProduceTheSameKey()
    {
        // Act
        string firstResult = PlaybackFormatCatalog.BuildPairKey("mp4", "aac");
        string secondResult = PlaybackFormatCatalog.BuildPairKey(" MP4 ", " AAC ");

        // Assert
        Assert.Equal(firstResult, secondResult);
    }

    [Theory]
    [InlineData(null, null, "/")] // both parts are absent
    [InlineData("", "", "/")] // both parts are empty
    [InlineData("   ", "   ", "/")] // both parts are white space
    [InlineData(null, "aac", "/AAC")] // only the container is absent
    [InlineData("mp4", null, "MP4/")] // only the codec is absent
    public void BuildPairKey_WhenAContainerOrCodecIsNullOrEmpty_ShouldProduceTheSlashSeparatedFormWithoutThrowing(string? container, string? codec, string expectedKey)
    {
        // Act
        string result = PlaybackFormatCatalog.BuildPairKey(container!, codec!);

        // Assert
        Assert.Equal(expectedKey, result);
    }

    [Fact]
    public void Formats_WhenRead_ShouldExposeANonEmptyMuxerALeadingDotExtensionAndANonEmptyEncoderForEveryFormat()
    {
        // Act
        IReadOnlyList<NormalizedAudioFormatDto> result = PlaybackFormatCatalog.Formats;

        // Assert
        Assert.All(result, format =>
        {
            Assert.False(string.IsNullOrWhiteSpace(format.MuxerName));
            Assert.StartsWith(".", format.FileExtension, StringComparison.Ordinal);
            Assert.True(format.FileExtension.Length > 1);
            Assert.False(string.IsNullOrWhiteSpace(format.EncoderName));
        });
    }
}
