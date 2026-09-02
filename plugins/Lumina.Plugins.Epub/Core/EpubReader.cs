#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Reading;
using Lumina.Plugins.Epub.Core.Epub;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Epub.Core;

/// <summary>
/// Book reader for the EPUB format.
/// </summary>
/// <remarks>
/// See <see cref="EpubDocumentParser"/> for what an EPUB is, and how it is decoded; this class only declares the supported format and delegates the decoding to the parser.
/// </remarks>
internal sealed class EpubReader : IBookReader
{
    /// <summary>
    /// Gets the file extensions supported by the reader.
    /// </summary>
    public IReadOnlyList<string> SupportedExtensions => [".epub"];

    /// <summary>
    /// Gets the media library types the reader supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.EBook, LibraryType.Book];

    /// <summary>
    /// Opens the EPUB stored at <paramref name="path"/>, extracting its sections and resources into
    /// <paramref name="workingDirectory"/>, and returns its normalized reading document.
    /// </summary>
    /// <param name="path">The file system path of the EPUB.</param>
    /// <param name="workingDirectory">The directory into which the sections and resources of the EPUB are extracted.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the book, when it is a PDF, is rendered as page images instead of extracting its text layer. An EPUB always has a text layer, so this is ignored.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The normalized reading document of the EPUB.</returns>
    public Task<ReadingDocumentDto> OpenAsync(string path, string workingDirectory, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
    {
        return Task.FromResult(EpubDocumentParser.Parse(path, workingDirectory, cancellationToken));
    }

    /// <summary>
    /// Produces the resource identified by <paramref name="resourceKey"/> of the EPUB stored at <paramref name="path"/>.
    /// Every resource of an EPUB is extracted to the working directory when the book is opened, so this reads the
    /// already extracted file. The resource key is the safe file name the parser derives from the manifest href, and the
    /// file is contained-checked by the host before it is served, so the key cannot escape the resources directory.
    /// </summary>
    /// <param name="path">The file system path of the EPUB.</param>
    /// <param name="workingDirectory">The directory into which the sections and resources of the EPUB are extracted.</param>
    /// <param name="resourceKey">The opaque resource key of the resource to produce.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The binary data of the extracted resource.</returns>
    public Task<byte[]> GetResourceAsync(string path, string workingDirectory, string resourceKey, CancellationToken cancellationToken)
    {
        byte[] data = File.ReadAllBytes(Path.Combine(workingDirectory, "resources", resourceKey));
        return Task.FromResult(data);
    }
}
