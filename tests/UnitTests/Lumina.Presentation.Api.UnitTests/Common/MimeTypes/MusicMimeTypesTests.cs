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
}
