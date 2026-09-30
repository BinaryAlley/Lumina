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
/// Contains integration tests for the <see cref="Id3ArtistMetadataProvider"/> class, reading real tagged audio containers.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3ArtistMetadataProviderTests : IDisposable
{
    private readonly ArtistMetadataLookupDtoFixture _artistMetadataLookupDtoFixture = new();
    private readonly Id3ArtistMetadataProvider _sut = new(new Id3TagReader());
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Id3ArtistMetadataProviderTests"/> class.
    /// </summary>
    public Id3ArtistMetadataProviderTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-id3-artist-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTagsCarryAnArtist_ShouldReturnTheMappedArtist()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "tagged.wav");
        Guid artistId = Guid.NewGuid();
        TestAudioFileFactory.CreateTaggedWav(path, file =>
        {
            file.Tag.Performers = ["The Artist"];
            file.Tag.PerformersSort = ["Artist, The"];
            file.Tag.MusicBrainzArtistId = artistId.ToString();
        });
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: path, name: "The Artist");

        // Act
        ArtistMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("The Artist", result.Name);
        Assert.Equal("Artist, The", result.SortName);
        Assert.Equal(artistId, result.MusicBrainzArtistId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheTagsCarryNoArtist_ShouldReturnNull()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "untagged.wav");
        TestAudioFileFactory.CreateWav(path);
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: path, name: "The Artist");

        // Act
        ArtistMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheTagsCarryAnArtist_ShouldReturnASingleCandidate()
    {
        // Arrange
        string path = Path.Combine(_temporaryDirectory, "tagged.wav");
        TestAudioFileFactory.CreateTaggedWav(path, file => file.Tag.Performers = ["The Artist"]);
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: path, name: "The Artist");

        // Act
        IReadOnlyList<ArtistMetadataDto> result = await _sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        ArtistMetadataDto candidate = Assert.Single(result);
        Assert.Equal("The Artist", candidate.Name);
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
