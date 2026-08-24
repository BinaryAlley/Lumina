#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using Lumina.Plugins.Calibre.Core.Opf;
using Lumina.Plugins.Contracts.Core.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Calibre.Core;

/// <summary>
/// Provides the cover of a book from the cover reference of the Calibre OPF file located next to the book file.
/// </summary>
internal sealed class CalibreArtworkProvider : IArtworkProvider<BookMetadataLookupDto>
{
    /// <summary>
    /// Gets the display name of the artwork provider.
    /// </summary>
    public string Name => "Calibre";

    /// <summary>
    /// Gets the media library types this artwork provider supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.Book, LibraryType.EBook];

    /// <summary>
    /// Gets a value indicating whether this artwork provider requires access to the web to retrieve artwork.
    /// </summary>
    public bool RequiresWebAccess => false;

    /// <summary>
    /// Gets the cover of the book described by <paramref name="lookup"/>.
    /// </summary>
    /// <param name="lookup">The lookup describing the book to get the cover for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The cover of the book, or <see langword="null"/> when no cover was found.</returns>
    public Task<ArtworkDto?> GetArtworkAsync(BookMetadataLookupDto lookup, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(lookup.Path))
            return Task.FromResult<ArtworkDto?>(null);

        string bookDirectory = Path.GetDirectoryName(lookup.Path) ?? string.Empty;
        string opfFilePath = Path.Combine(bookDirectory, "metadata.opf");
        if (!File.Exists(opfFilePath))
            return Task.FromResult<ArtworkDto?>(null);

        OpfDocumentDto document = OpfReader.Read(opfFilePath);
        if (string.IsNullOrWhiteSpace(document.CoverHref))
            return Task.FromResult<ArtworkDto?>(null);

        // the cover reference must be a bare file name contained in the directory of the book, so that no file outside of it can be accessed
        string coverFileName = Path.GetFileName(document.CoverHref);
        if (coverFileName != document.CoverHref || Path.IsPathRooted(document.CoverHref))
            return Task.FromResult<ArtworkDto?>(null);

        string coverFilePath = Path.Combine(bookDirectory, coverFileName);
        if (!File.Exists(coverFilePath))
            return Task.FromResult<ArtworkDto?>(null);

        return Task.FromResult<ArtworkDto?>(new ArtworkDto(LocalPath: coverFilePath, RemoteUrl: null));
    }
}
