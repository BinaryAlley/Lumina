#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.Core.Metadata;

/// <summary>
/// Contract for a plugin that provides artwork for media items (i.e., covers, banners, fanart, etc).
/// </summary>
public interface IArtworkProvider
{
    /// <summary>
    /// Gets the display name of the artwork provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the media library types this artwork provider supports.
    /// </summary>
    IReadOnlyList<LibraryType> SupportedLibraryTypes { get; }

    /// <summary>
    /// Gets a value indicating whether this artwork provider requires access to the web to retrieve artwork.
    /// </summary>
    bool RequiresWebAccess { get; }

    /// <summary>
    /// Gets the artwork of the media item described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item to get the artwork for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The artwork of the media item, or <see langword="null"/> when no artwork was found.</returns>
    Task<ArtworkDto?> GetArtworkAsync(MetadataLookupDto lookup, CancellationToken cancellationToken);
}
