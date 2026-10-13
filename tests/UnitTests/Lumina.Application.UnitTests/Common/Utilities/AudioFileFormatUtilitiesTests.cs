#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="AudioFileFormatUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioFileFormatUtilitiesTests
{
    [Theory]
    [InlineData("track.mp3", AudioFileFormat.MP3)] // mp3 audio file
    [InlineData("track.flac", AudioFileFormat.FLAC)] // flac audio file
    [InlineData("track.m4a", AudioFileFormat.M4A)] // m4a audio file
    [InlineData("track.aac", AudioFileFormat.AAC)] // aac audio file
    [InlineData("track.ogg", AudioFileFormat.OGG)] // ogg audio file
    [InlineData("track.opus", AudioFileFormat.OPUS)] // opus audio file
    [InlineData("track.wav", AudioFileFormat.WAV)] // wav audio file
    [InlineData("track.wma", AudioFileFormat.WMA)] // wma audio file
    [InlineData("track.aiff", AudioFileFormat.AIFF)] // aiff audio file
    [InlineData("track.aif", AudioFileFormat.AIFF)] // aif audio file
    [InlineData("track.ape", AudioFileFormat.APE)] // ape audio file
    [InlineData("track.wv", AudioFileFormat.WAVPACK)] // wavpack audio file
    [InlineData("track.dsf", AudioFileFormat.DSF)] // dsf audio file
    [InlineData("track.dff", AudioFileFormat.DFF)] // dff audio file
    [InlineData("track.mka", AudioFileFormat.MATROSKA)] // matroska audio file
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
}
