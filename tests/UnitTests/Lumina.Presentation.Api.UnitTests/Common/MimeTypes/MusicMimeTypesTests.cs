#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Presentation.Api.Common.MimeTypes;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Common.MimeTypes;

/// <summary>
/// Contains unit tests for the <see cref="MusicMimeTypes"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMimeTypesTests
{
    [Theory]
    [InlineData(AudioFileFormat.MP3, "audio/mpeg")] // mp3 audio format
    [InlineData(AudioFileFormat.FLAC, "audio/flac")] // flac audio format
    [InlineData(AudioFileFormat.M4A, "audio/mp4")] // m4a audio format
    [InlineData(AudioFileFormat.AAC, "audio/aac")] // aac audio format
    [InlineData(AudioFileFormat.OGG, "audio/ogg")] // ogg audio format
    [InlineData(AudioFileFormat.OPUS, "audio/ogg")] // opus audio format
    [InlineData(AudioFileFormat.WAV, "audio/wav")] // wav audio format
    [InlineData(AudioFileFormat.WMA, "audio/x-ms-wma")] // wma audio format
    [InlineData(AudioFileFormat.AIFF, "audio/aiff")] // aiff audio format
    [InlineData(AudioFileFormat.APE, "audio/x-ape")] // ape audio format
    [InlineData(AudioFileFormat.WAVPACK, "audio/x-wavpack")] // wavpack audio format
    [InlineData(AudioFileFormat.DSF, "audio/x-dsf")] // dsf audio format
    [InlineData(AudioFileFormat.DFF, "audio/x-dff")] // dff audio format
    [InlineData(AudioFileFormat.MATROSKA, "audio/x-matroska")] // matroska audio format
    public void GetMimeType_WhenFormatIsKnown_ShouldReturnMappedMimeType(AudioFileFormat format, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetMimeType(format);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(AudioFileFormat.None)] // the none audio format has no mime mapping
    [InlineData((AudioFileFormat)999)] // an unknown audio format has no mime mapping
    public void GetMimeType_WhenFormatIsUnknown_ShouldReturnOpaqueBinaryMimeType(AudioFileFormat format)
    {
        // Act
        string result = MusicMimeTypes.GetMimeType(format);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Theory]
    [InlineData("MP3", "audio/mpeg")] // mp3 container
    [InlineData("FLAC", "audio/flac")] // flac container
    [InlineData("MP4", "audio/mp4")] // mp4 container
    [InlineData("M4A", "audio/mp4")] // m4a container
    [InlineData("AAC", "audio/aac")] // aac container
    [InlineData("OGG", "audio/ogg")] // ogg container
    [InlineData("OPUS", "audio/ogg")] // opus container
    [InlineData("WAV", "audio/wav")] // wav container
    public void GetContainerMimeType_WhenContainerIsMapped_ShouldReturnMappedMimeType(string container, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData("mp3", "audio/mpeg")] // lowercase mp3 container
    [InlineData("Flac", "audio/flac")] // mixed case flac container
    [InlineData("mP4", "audio/mp4")] // mixed case mp4 container
    [InlineData("opus", "audio/ogg")] // lowercase opus container
    public void GetContainerMimeType_WhenContainerDiffersInCase_ShouldReturnMappedMimeType(string container, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(" MP3 ", "audio/mpeg")] // leading and trailing whitespace around mp3
    [InlineData("\tOGG", "audio/ogg")] // leading tab around ogg
    [InlineData("WAV\n", "audio/wav")] // trailing newline around wav
    public void GetContainerMimeType_WhenContainerHasSurroundingWhitespace_ShouldReturnMappedMimeType(string container, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(null)] // an absent container has no mime mapping
    [InlineData("")] // an empty container has no mime mapping
    [InlineData("   ")] // a whitespace only container has no mime mapping
    [InlineData("WMA")] // a known file format that is not a mapped delivery container
    [InlineData("not-a-container")] // an unknown container has no mime mapping
    public void GetContainerMimeType_WhenContainerIsUnknown_ShouldReturnOpaqueBinaryMimeType(string? container)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }
}
