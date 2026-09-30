#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Plugins.ID3.Core;
using Lumina.Plugins.ID3.Core.Tags;
using Lumina.Plugins.ID3.Fixtures.Core.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.ID3.IntegrationTests.Core;

/// <summary>
/// Contains integration tests for the <see cref="Id3TrackMetadataProvider"/> class, reading real tagged audio containers.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3TrackMetadataProviderTests : IDisposable
{
    private readonly TrackMetadataLookupDtoFixture _trackMetadataLookupDtoFixture = new();
    private readonly Id3TrackMetadataProvider _sut = new(new Id3TagReader());
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Id3TrackMetadataProviderTests"/> class.
    /// </summary>
    public Id3TrackMetadataProviderTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-id3-track-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTagsCarryATitle_ShouldReturnTheMappedTrack()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "tagged.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            file.Tag.Title = "The Track";
            file.Tag.InitialKey = "C#m";
            file.Tag.BeatsPerMinute = 128;
            file.Tag.Performers = ["The Artist"];
        });
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: path, title: "The Track");

        // Act
        AudioMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Track", result.Title);
        Assert.Equal(MusicKey.CSharpMinor, result.Key);
        Assert.Equal(128, result.Bpm);
        Assert.NotNull(result.Contributors);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Artist");
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTagsCarryNoTitle_ShouldReturnNull()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "untagged.wav");
        TestAudioFileFactory.CreateWav(path);
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: path, title: "The Track");

        // Act
        AudioMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheTagsCarryATitle_ShouldReturnASingleCandidate()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "tagged.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => file.Tag.Title = "The Track");
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: path, title: "The Track");

        // Act
        IReadOnlyList<AudioMetadataDto> result = await _sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        AudioMetadataDto candidate = Assert.Single(result);
        Assert.Equal("The Track", candidate.Title);
    }

    /// <summary>
    /// Removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
        GC.SuppressFinalize(this);
    }
}
