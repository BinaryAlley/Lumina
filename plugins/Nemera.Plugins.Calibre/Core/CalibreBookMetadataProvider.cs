#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Calibre.Core.Mapping;
using Lumina.Plugins.Calibre.Core.Opf;
using Lumina.Plugins.Contracts.Core.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Calibre.Core;

/// <summary>
/// Provides book metadata from Calibre OPF files by resolving lookups into book metadata DTOs.
/// </summary>
internal sealed class CalibreBookMetadataProvider : IMetadataProvider<BookMetadataLookupDto, BookMetadataDto>
{
    /// <summary>
    /// Gets the display name of the metadata provider.
    /// </summary>
    public string Name => "Calibre";

    /// <summary>
    /// Gets the media library types this metadata provider supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Book, LibraryType.EBook];

    /// <summary>
    /// Gets a value indicating whether this metadata provider requires access to the web to retrieve metadata.
    /// </summary>
    public bool RequiresWebAccess => false;

    /// <summary>
    /// Searches for the metadata of the media item described by <paramref name="bookMetadataLookup"/>.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup describing the media item to search for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The collection of metadata candidates found for the media item.</returns>
    public Task<IReadOnlyList<BookMetadataDto>> GetSearchResultsAsync(BookMetadataLookupDto bookMetadataLookup, CancellationToken cancellationToken)
    {
        BookMetadataDto? metadata = GetMetadata(bookMetadataLookup);
        return Task.FromResult<IReadOnlyList<BookMetadataDto>>(metadata is null ? [] : [metadata]);
    }

    /// <summary>
    /// Gets the metadata of the media item described by <paramref name="bookMetadataLookup"/>.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup describing the media item to get the metadata for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The metadata of the media item, or <see langword="null"/> when no metadata was found.</returns>
    public Task<BookMetadataDto?> GetMetadataAsync(BookMetadataLookupDto bookMetadataLookup, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetMetadata(bookMetadataLookup));
    }

    /// <summary>
    /// Reads the metadata of the book from the OPF file located next to the book file described by <paramref name="bookMetadataLookup"/>.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup describing the book to get the metadata for.</param>
    /// <returns>The metadata of the book, or <see langword="null"/> when no OPF file was found.</returns>
    private static BookMetadataDto? GetMetadata(BookMetadataLookupDto bookMetadataLookup)
    {
        if (string.IsNullOrWhiteSpace(bookMetadataLookup.Path))
            return null;

        string opfFilePath = Path.Combine(Path.GetDirectoryName(bookMetadataLookup.Path) ?? string.Empty, "metadata.opf");
        if (!File.Exists(opfFilePath))
            return null;

        return CalibreMapper.Map(OpfReader.Read(opfFilePath));
    }
}
