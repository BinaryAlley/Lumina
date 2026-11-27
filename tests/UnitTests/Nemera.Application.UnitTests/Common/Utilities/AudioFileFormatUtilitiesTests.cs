#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Utilities;
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="AudioFileFormatUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioFileFormatUtilitiesTests
{
    [Theory]
    [InlineData("track.mp3", AudioFileFormat.MP3)] // MP3 audio file.
    [InlineData("track.flac", AudioFileFormat.FLAC)] // FLAC audio file.
    [InlineData("track.m4a", AudioFileFormat.M4A)] // M4A audio file.
    [InlineData("track.aac", AudioFileFormat.AAC)] // AAC audio file.
    [InlineData("track.ogg", AudioFileFormat.OGG)] // OGG audio file.
    [InlineData("track.opus", AudioFileFormat.OPUS)] // Opus audio file.
    [InlineData("track.wav", AudioFileFormat.WAV)] // WAV audio file.
    [InlineData("track.wma", AudioFileFormat.WMA)] // WMA audio file.
    [InlineData("track.aiff", AudioFileFormat.AIFF)] // AIFF audio file.
    [InlineData("track.aif", AudioFileFormat.AIFF)] // AIF audio file.
    [InlineData("track.ape", AudioFileFormat.APE)] // APE audio file.
    [InlineData("track.wv", AudioFileFormat.WAVPACK)] // WavPack audio file.
    [InlineData("track.dsf", AudioFileFormat.DSF)] // DSF audio file.
    [InlineData("track.dff", AudioFileFormat.DFF)] // DFF audio file.
    [InlineData("track.mka", AudioFileFormat.MATROSKA)] // Matroska audio file.
    public void ToAudioFileFormat_WhenExtensionIsKnown_ShouldReturnMappedFormat(string filePath, AudioFileFormat expectedFormat)
    {
        // Act
        AudioFileFormat result = filePath.ToAudioFileFormat();

        // Assert
        Assert.Equal(expectedFormat, result);
    }

    [Fact]
    public void ToAudioFileFormat_WhenExtensionIsUnknown_ShouldReturnNone()
    {
        // Arrange
        string filePath = "track.unknown";

        // Act
        AudioFileFormat result = filePath.ToAudioFileFormat();

        // Assert
        Assert.Equal(AudioFileFormat.None, result);
    }

    [Fact]
    public void ToAudioFileFormat_WhenFileHasNoExtension_ShouldReturnNone()
    {
        // Arrange
        string filePath = "track";

        // Act
        AudioFileFormat result = filePath.ToAudioFileFormat();

        // Assert
        Assert.Equal(AudioFileFormat.None, result);
    }

    [Fact]
    public void ToAudioFileFormat_WhenExtensionIsMixedCase_ShouldResolveCaseInsensitively()
    {
        // Arrange
        string filePath = "Track.MP3";

        // Act
        AudioFileFormat result = filePath.ToAudioFileFormat();

        // Assert
        Assert.Equal(AudioFileFormat.MP3, result);
    }

    [Fact]
    public void ToAudioFileFormat_WhenFilePathContainsDirectoriesAndMixedCaseExtension_ShouldReturnMappedFormat()
    {
        // Arrange
        string filePath = "library/music/Queen/A Night at the Opera (1975).Remastered/01 - Bohemian Rhapsody.FLAC";

        // Act
        AudioFileFormat result = filePath.ToAudioFileFormat();

        // Assert
        Assert.Equal(AudioFileFormat.FLAC, result);
    }
}
