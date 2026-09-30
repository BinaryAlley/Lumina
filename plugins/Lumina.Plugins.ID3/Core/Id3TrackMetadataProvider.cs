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
/// Provides track metadata from the embedded tags of the audio file, by resolving lookups into audio metadata DTOs.
/// </summary>
internal sealed class Id3TrackMetadataProvider : IMetadataProvider<TrackMetadataLookupDto, AudioMetadataDto>
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
    /// Initializes a new instance of the <see cref="Id3TrackMetadataProvider"/> class.
    /// </summary>
    /// <param name="tagReader">The reader of the embedded tags of the audio files.</param>
    public Id3TrackMetadataProvider(Id3TagReader tagReader)
    {
        _tagReader = tagReader;
    }

    /// <summary>
    /// Searches for the metadata of the track described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the track to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of track metadata candidates found, which for embedded tags is at most the file of the lookup.</returns>
    public async Task<IReadOnlyList<AudioMetadataDto>> GetSearchResultsAsync(TrackMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        AudioMetadataDto? metadata = await ReadMetadataAsync(lookup.Path, cancellationToken).ConfigureAwait(false);
        IReadOnlyList<AudioMetadataDto> results = metadata is null ? [] : [metadata];
        return results;
    }

    /// <summary>
    /// Gets the metadata of the track described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the track to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the track, or <see langword="null"/> when the file carries none.</returns>
    public Task<AudioMetadataDto?> GetMetadataAsync(TrackMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        return ReadMetadataAsync(lookup.Path, cancellationToken);
    }

    /// <summary>
    /// Reads the metadata of the track stored at <paramref name="path"/> from its embedded tags.
    /// </summary>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the track, or <see langword="null"/> when the tags carry no title.</returns>
    private Task<AudioMetadataDto?> ReadMetadataAsync(string path, CancellationToken cancellationToken)
    {
        // The tagging library exposes no asynchronous API, so the blocking read of the file is offloaded so that a scan does not block on the tags of each file.
        return Task.Run(() =>
        {
            Id3TagDto? data = _tagReader.Read(path);
            if (data is null || string.IsNullOrWhiteSpace(data.Title))
                return null;
            return Id3TagMapper.MapTrack(data);
        }, cancellationToken);
    }
}
