#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Reading;
using Lumina.Plugins.Pdf.Core.Pdf;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Pdf.Core;

/// <summary>
/// Book reader for the PDF format.
/// </summary>
/// <remarks>
/// See <see cref="PdfDocumentParser"/> for how a PDF is decoded; this class only declares the supported format and delegates the decoding to the parser.
/// </remarks>
internal sealed class PdfReader : IBookReader
{
    /// <summary>
    /// Gets the file extensions supported by the reader.
    /// </summary>
    public IReadOnlyList<string> SupportedExtensions => [".pdf"];

    /// <summary>
    /// Gets the media library types the reader supports.
    /// </summary>
    public IReadOnlyList<LibraryType> SupportedLibraryTypes => [LibraryType.EBook, LibraryType.Book];

    /// <summary>
    /// Opens the PDF stored at <paramref name="path"/>, extracting its pages into <paramref name="workingDirectory"/>,
    /// and returns its normalized reading document. When the PDF is rendered as page images, the page images are not rendered
    /// here: a page is rendered only when its resource is requested, so that opening a large book is instant.
    /// </summary>
    /// <param name="path">The file system path of the PDF.</param>
    /// <param name="workingDirectory">The directory into which the pages of the PDF are extracted.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the PDF is rendered as page images instead of extracting its text layer. This is used for scanned PDFs whose pages are only images and therefore yield no text.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The normalized reading document of the PDF.</returns>
    public Task<ReadingDocumentDto> OpenAsync(string path, string workingDirectory, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
    {
        return Task.FromResult(PdfDocumentParser.Parse(path, workingDirectory, shouldRenderPdfAsImages, cancellationToken));
    }

    /// <summary>
    /// Produces the resource identified by <paramref name="resourceKey"/> of the PDF stored at <paramref name="path"/>,
    /// rendering the requested page to an image on demand. Only the page image whose resource is requested is rendered,
    /// so that the user does not wait for every page of a large book to be rendered before seeing the first one.
    /// </summary>
    /// <param name="path">The file system path of the PDF.</param>
    /// <param name="workingDirectory">The directory into which the pages of the PDF are extracted.</param>
    /// <param name="resourceKey">The opaque resource key of the resource to produce, of the form <c>page:{n}</c>.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The binary data of the rendered page image.</returns>
    /// <remarks>
    /// The PDFtoImage and SkiaSharp APIs used by <see cref="PdfDocumentParser.RenderResource"/> are annotated with the supported
    /// platforms of the rendering engine, so this method must repeat that restriction or the CA1416 analyzer, which is a build error
    /// here, flags the call. The attributes must not be removed, even though Windows, Linux and macOS are the only platforms Lumina runs on.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public Task<byte[]> GetResourceAsync(string path, string workingDirectory, string resourceKey, CancellationToken cancellationToken)
    {
        byte[] data = PdfDocumentParser.RenderResource(path, resourceKey, cancellationToken);
        return Task.FromResult(data);
    }
}
