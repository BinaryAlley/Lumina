#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.Core.Reading;

/// <summary>
/// Contract for a plugin that decodes a book format into a normalized reading document.
/// </summary>
public interface IBookReader
{
    /// <summary>
    /// Gets the file extensions supported by the reader, with the leading dot (i.e., <c>.epub</c>).
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Gets the media library types the reader supports.
    /// </summary>
    IReadOnlyList<LibraryType> SupportedLibraryTypes { get; }

    /// <summary>
    /// Opens the book stored at <paramref name="path"/>, extracting its sections and resources into
    /// <paramref name="workingDirectory"/>, and returns its normalized reading document.
    /// </summary>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="workingDirectory">The directory into which the sections and resources of the book are extracted.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The normalized reading document of the book.</returns>
    Task<ReadingDocumentDto> OpenAsync(string path, string workingDirectory, bool shouldRenderPdfAsImages, CancellationToken cancellationToken);

    /// <summary>
    /// Produces the bytes of the resource identified by <paramref name="resourceKey"/> of the book stored at <paramref name="path"/>, on demand.
    /// A reader that extracts every resource eagerly (for example an EPUB, whose resources are read straight from the archive) can
    /// simply return the bytes of the extracted file; a reader that renders resources lazily (for example a PDF whose pages are
    /// rendered as images only when they are shown) produces the resource here. The media type of the resource is not returned by the
    /// reader: the host already knows it from the manifest of the book, and serves the resource with that media type.
    /// </summary>
    /// <param name="path">The file system path of the book.</param>
    /// <param name="workingDirectory">The directory into which the sections and resources of the book are extracted.</param>
    /// <param name="resourceKey">The opaque resource key of the resource to produce.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The binary data of the produced resource.</returns>
    Task<byte[]> GetResourceAsync(string path, string workingDirectory, string resourceKey, CancellationToken cancellationToken);
}
