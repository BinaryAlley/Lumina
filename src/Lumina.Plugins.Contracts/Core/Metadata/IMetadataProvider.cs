#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.Core.Metadata;

/// <summary>
/// Contract for a plugin that provides metadata for media items.
/// </summary>
public interface IMetadataProvider
{
    /// <summary>
    /// Gets the display name of the metadata provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the media library types this metadata provider supports.
    /// </summary>
    IReadOnlyList<LibraryType> SupportedLibraryTypes { get; }

    /// <summary>
    /// Gets a value indicating whether this metadata provider requires access to the web to retrieve metadata.
    /// </summary>
    bool RequiresWebAccess { get; }

    /// <summary>
    /// Searches for the metadata of the media item described by <paramref name="metadataLookup"/>.
    /// </summary>
    /// <param name="metadataLookup">The lookup describing the media item to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of metadata candidates found for the media item.</returns>
    Task<IReadOnlyList<MetadataDto>> GetSearchResultsAsync(MetadataLookupDto metadataLookup, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the metadata of the media item described by <paramref name="metadataLookup"/>.
    /// </summary>
    /// <param name="metadataLookup">The lookup describing the media item to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the media item, or <see langword="null"/> when no metadata was found.</returns>
    Task<MetadataDto?> GetMetadataAsync(MetadataLookupDto metadataLookup, CancellationToken cancellationToken);
}
