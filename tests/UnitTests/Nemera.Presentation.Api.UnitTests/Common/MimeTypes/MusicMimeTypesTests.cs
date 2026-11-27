#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Nemera.Presentation.Api.Common.MimeTypes;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Api.UnitTests.Common.MimeTypes;

/// <summary>
/// Contains unit tests for the <see cref="MusicMimeTypes"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMimeTypesTests
{
    [Theory]
    [InlineData(AudioFileFormat.MP3, "audio/mpeg")] // MP3 audio format.
    [InlineData(AudioFileFormat.FLAC, "audio/flac")] // FLAC audio format.
    [InlineData(AudioFileFormat.M4A, "audio/mp4")] // M4A audio format.
    [InlineData(AudioFileFormat.AAC, "audio/aac")] // AAC audio format.
    [InlineData(AudioFileFormat.OGG, "audio/ogg")] // OGG audio format.
    [InlineData(AudioFileFormat.OPUS, "audio/ogg")] // Opus audio format.
    [InlineData(AudioFileFormat.WAV, "audio/wav")] // WAV audio format.
    [InlineData(AudioFileFormat.WMA, "audio/x-ms-wma")] // WMA audio format.
    [InlineData(AudioFileFormat.AIFF, "audio/aiff")] // AIFF audio format.
    [InlineData(AudioFileFormat.APE, "audio/x-ape")] // APE audio format.
    [InlineData(AudioFileFormat.WAVPACK, "audio/x-wavpack")] // WavPack audio format.
    [InlineData(AudioFileFormat.DSF, "audio/x-dsf")] // DSF audio format.
    [InlineData(AudioFileFormat.DFF, "audio/x-dff")] // DFF audio format.
    [InlineData(AudioFileFormat.MATROSKA, "audio/x-matroska")] // Matroska audio format.
    public void GetMimeType_WhenFormatIsKnown_ShouldReturnMappedMimeType(AudioFileFormat format, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetMimeType(format);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(AudioFileFormat.None)] // The none audio format has no mime mapping.
    [InlineData((AudioFileFormat)999)] // An unknown audio format has no mime mapping.
    public void GetMimeType_WhenFormatIsUnknown_ShouldReturnOpaqueBinaryMimeType(AudioFileFormat format)
    {
        // Act
        string result = MusicMimeTypes.GetMimeType(format);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Theory]
    [InlineData("MP3", "audio/mpeg")] // MP3 container.
    [InlineData("FLAC", "audio/flac")] // FLAC container.
    [InlineData("MP4", "audio/mp4")] // MP4 container.
    [InlineData("M4A", "audio/mp4")] // M4A container.
    [InlineData("AAC", "audio/aac")] // AAC container.
    [InlineData("OGG", "audio/ogg")] // OGG container.
    [InlineData("OPUS", "audio/ogg")] // Opus container.
    [InlineData("WAV", "audio/wav")] // WAV container.
    public void GetContainerMimeType_WhenContainerIsMapped_ShouldReturnMappedMimeType(string container, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData("mp3", "audio/mpeg")] // Lowercase MP3 container.
    [InlineData("Flac", "audio/flac")] // Mixed case FLAC container.
    [InlineData("mP4", "audio/mp4")] // Mixed case MP4 container.
    [InlineData("opus", "audio/ogg")] // Lowercase Opus container.
    public void GetContainerMimeType_WhenContainerDiffersInCase_ShouldReturnMappedMimeType(string container, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(" MP3 ", "audio/mpeg")] // Leading and trailing whitespace around MP3.
    [InlineData("\tOGG", "audio/ogg")] // Leading tab around OGG.
    [InlineData("WAV\n", "audio/wav")] // Trailing newline around WAV.
    public void GetContainerMimeType_WhenContainerHasSurroundingWhitespace_ShouldReturnMappedMimeType(string container, string expectedMimeType)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(null)] // An absent container has no mime mapping.
    [InlineData("")] // An empty container has no mime mapping.
    [InlineData("   ")] // A whitespace only container has no mime mapping.
    [InlineData("WMA")] // A known file format that is not a mapped delivery container.
    [InlineData("not-a-container")] // An unknown container has no mime mapping.
    public void GetContainerMimeType_WhenContainerIsUnknown_ShouldReturnOpaqueBinaryMimeType(string? container)
    {
        // Act
        string result = MusicMimeTypes.GetContainerMimeType(container);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }
}
