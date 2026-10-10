#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.ID3.Common.Models.DTO.Tags;
using Lumina.Plugins.ID3.Core.Mapping;
using Lumina.Plugins.ID3.Core.Tags;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.ID3.Core;

/// <summary>
/// Provides music artist metadata from the embedded tags of one of its audio files, by resolving lookups into artist metadata DTOs.
/// </summary>
internal sealed class Id3ArtistMetadataProvider : IMetadataProvider<ArtistMetadataLookupDto, ArtistMetadataDto>
{
    private readonly Id3TagReader _tagReader;

    /// <summary>
    /// Gets the display name of the metadata provider.
    /// </summary>
    public string Name => "ID3";

    /// <summary>
    /// Gets the media library types this metadata provider supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Music];

    /// <summary>
    /// Gets a value indicating whether this metadata provider requires access to the web to retrieve metadata.
    /// </summary>
    public bool RequiresWebAccess => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Id3ArtistMetadataProvider"/> class.
    /// </summary>
    /// <param name="tagReader">The reader of the embedded tags of the audio files.</param>
    public Id3ArtistMetadataProvider(Id3TagReader tagReader)
    {
        _tagReader = tagReader;
    }

    /// <summary>
    /// Searches for the metadata of the artist described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the artist to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of artist metadata candidates found, which for embedded tags is at most the file of the lookup.</returns>
    public async Task<IReadOnlyList<ArtistMetadataDto>> GetSearchResultsAsync(ArtistMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        ArtistMetadataDto? metadata = await ReadMetadataAsync(lookup, cancellationToken).ConfigureAwait(false);
        IReadOnlyList<ArtistMetadataDto> results = metadata is null ? [] : [metadata];
        return results;
    }

    /// <summary>
    /// Gets the metadata of the artist described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the artist to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the artist, or <see langword="null"/> when the file carries none.</returns>
    public Task<ArtistMetadataDto?> GetMetadataAsync(ArtistMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return ReadMetadataAsync(lookup, cancellationToken);
    }

    /// <summary>
    /// Reads the metadata of the artist described by <paramref name="lookup"/> from the embedded tags of the track stored at its path.
    /// </summary>
    /// <param name="lookup">The lookup describing the artist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the artist, or <see langword="null"/> when the tags carry no artist name.</returns>
    private Task<ArtistMetadataDto?> ReadMetadataAsync(ArtistMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        // The tagging library exposes no asynchronous API, so the blocking read of the file is offloaded so that a scan does not block on the tags of each file.
        return Task.Run(() =>
        {
            Id3TagDto? data = _tagReader.Read(lookup.Path);
            if (data is null || (data.AlbumArtists.Count == 0 && data.TrackArtists.Count == 0))
                return null;
            return Id3TagMapper.MapArtist(data, lookup.Name);
        }, cancellationToken);
    }
}
