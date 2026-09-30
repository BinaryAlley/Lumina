#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
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
/// Contains integration tests for the <see cref="Id3AlbumMetadataProvider"/> class, reading real tagged audio containers.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3AlbumMetadataProviderTests : IDisposable
{
    private readonly AlbumMetadataLookupDtoFixture _albumMetadataLookupDtoFixture = new();
    private readonly Id3AlbumMetadataProvider _sut = new(new Id3TagReader());
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Id3AlbumMetadataProviderTests"/> class.
    /// </summary>
    public Id3AlbumMetadataProviderTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-id3-album-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTagsCarryAnAlbum_ShouldReturnTheMappedAlbum()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "tagged.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            file.Tag.Album = "The Album";
            file.Tag.AlbumArtists = ["The Album Artist"];
            file.Tag.TrackCount = 10;
            file.Tag.DiscCount = 2;
        });
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: path, title: "The Album");

        // Act
        AlbumMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Album", result.Title);
        Assert.Equal(10, result.TotalTracks);
        Assert.Equal(2, result.TotalDiscs);
        Assert.NotNull(result.Contributors);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Album Artist");
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTagsCarryNoAlbum_ShouldReturnNull()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "untagged.wav");
        TestAudioFileFactory.CreateWav(path);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: path, title: "The Album");

        // Act
        AlbumMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheTagsCarryAnAlbum_ShouldReturnASingleCandidate()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "tagged.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => file.Tag.Album = "The Album");
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: path, title: "The Album");

        // Act
        IReadOnlyList<AlbumMetadataDto> result = await _sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        AlbumMetadataDto candidate = Assert.Single(result);
        Assert.Equal("The Album", candidate.Title);
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
