#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using PDFtoImage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Outline;
#endregion

namespace Lumina.Plugins.Pdf.Core.Pdf;

/// <summary>
/// Parses a PDF into a normalized reading document, extracting the text of its pages into a working directory.
/// </summary>
/// <remarks>
/// A PDF is a fixed-layout document, that positions its content on pages that never change. The reading order of a PDF is simply page 1, page 2, page 3, and so on, so this reader maps each page to one
/// reading section whose location reference is its page number ("page:{n}").
///
/// PdfPig reads the text layer of the pages, that is, the text that the PDF encodes for selection and search. A scanned book has no text layer, only page images, so its pages yield no text.
/// The table of contents, when the PDF has one, comes from its bookmarks (the document outline), where each bookmark points to a page.
/// </remarks>
internal static class PdfDocumentParser
{
    // A PDF is parsed only once per book, and the pages are extracted to disk and served from there, so the in-memory footprint stays bounded regardless of the size of the book;
    // the file size and page count are still capped so that a hostile document cannot exhaust the disk or the CPU of the host.
    private const long MAX_PDF_FILE_SIZE_BYTES = 500 * 1024 * 1024;
    private const int MAX_PAGES = 10_000;
    // PDFtoImage renders at the natural size of the page, so the pixel size of the bitmap follows the page size in points: a hostile
    // page whose MediaBox declares thousands of points would otherwise allocate a multi-gigabyte bitmap. Rendering is therefore limited
    // to pages of at most 4000 points on each side and at most 9,000,000 points squared in area, which keeps the rendered bitmap, four
    // bytes per pixel, bounded at a few tens of megabytes.
    private const int MAX_RENDER_PAGE_DIMENSION = 4000;
    private const int MAX_RENDER_PAGE_AREA = 9_000_000;

    /// <summary>
    /// Parses the PDF stored at <paramref name="pdfPath"/>, extracting its pages into <paramref name="workingDirectory"/>, and returns its normalized reading document.
    /// </summary>
    /// <param name="pdfPath">The file system path of the PDF.</param>
    /// <param name="workingDirectory">The directory into which the pages of the PDF are extracted.</param>
    /// <param name="shouldRenderPdfAsImages">Whether the PDF is rendered as page images instead of extracting its text layer. This is used for scanned PDFs whose pages are only images and therefore yield no text.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The normalized reading document of the PDF.</returns>
    public static ReadingDocumentDto Parse(string pdfPath, string workingDirectory, bool shouldRenderPdfAsImages, CancellationToken cancellationToken)
    {
        FileInfo fileInfo = new(pdfPath);
        if (!fileInfo.Exists)
            throw new FileNotFoundException("The PDF file could not be found.", pdfPath);
        if (fileInfo.Length > MAX_PDF_FILE_SIZE_BYTES)
            throw new InvalidDataException("The PDF file is too large.");

        // The pages are extracted into the sections directory, and, when the PDF is rendered as images, the page images into the
        // resources directory, from which they are served like any other book resource.
        string sectionsDirectory = Path.Combine(workingDirectory, "sections");
        Directory.CreateDirectory(sectionsDirectory);

        // When the PDF is rendered as images, the text layer is not read at all: a scanned PDF has no text layer, so reading it
        // would produce nothing, and rendering the pages as images is the only way to make such a book readable.
        if (shouldRenderPdfAsImages)
            return ParseAsImages(pdfPath, sectionsDirectory, cancellationToken);

        return ParseAsText(pdfPath, sectionsDirectory, cancellationToken);
    }

    /// <summary>
    /// Parses the PDF stored at <paramref name="pdfPath"/> by enumerating its pages, registering each page as a section whose only
    /// element references the page image, and returns its normalized reading document. The page images are not rendered here: a page
    /// is rendered to an image only when its resource is requested, so that opening a large book is instant and the user does not
    /// wait for every page to be rendered before seeing the first one.
    /// </summary>
    /// <param name="pdfPath">The file system path of the PDF.</param>
    /// <param name="sectionsDirectory">The directory into which the section files of the PDF are written.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The normalized reading document of the PDF, without a text layer.</returns>
    private static ReadingDocumentDto ParseAsImages(string pdfPath, string sectionsDirectory, CancellationToken cancellationToken)
    {
        List<ReadingSpineItemDto> spine = [];
        Dictionary<string, ReadingResourceInfoDto> resources = [];
        List<ReadingTocEntryDto> tableOfContents = [];
        using (PdfDocument pdfDocument = PdfDocument.Open(pdfPath))
        {
            foreach (Page page in pdfDocument.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (page.Number > MAX_PAGES)
                    break;

                // The location reference of a page is its 1-based page number (PdfPig pages are numbered from 1), which is the only
                // stable identifier a PDF page has, and it doubles as the name of the section file. A page rendered to an image has
                // no text, so its section is a single image element pointing at the rendered page resource.
                string locationRef = $"page:{page.Number}";
                string resourceKey = $"page:{page.Number}";
                string imageFileName = $"page-{page.Number}.png";
                string fileName = $"{page.Number}.html";
                string sectionHtml = $"<section><img data-lumina-resource=\"{resourceKey}\" alt=\"{WebUtility.HtmlEncode(locationRef)}\" /></section>";
                File.WriteAllText(Path.Combine(sectionsDirectory, fileName), sectionHtml, Encoding.UTF8);
                spine.Add(new ReadingSpineItemDto(locationRef, null, $"sections/{fileName}"));
                // The resource is registered with its expected file path, but the image is rendered lazily, when the resource is
                // requested, so that opening the book does not render pages the user never looks at.
                resources[resourceKey] = new ReadingResourceInfoDto($"resources/{imageFileName}", "image/png");
            }

            // The table of contents is read from the document that was already opened for the pages, so the PDF is not opened twice.
            tableOfContents = ReadTableOfContents(pdfDocument);
        }

        // The rendered pages have no text layer, and a PDF has no embedded resources to serve beyond the rendered pages.
        return new ReadingDocumentDto(Path.GetFileNameWithoutExtension(pdfPath), null, null, tableOfContents, spine, resources, HasTextContent: false);
    }

    /// <summary>
    /// Renders the page image identified by <paramref name="resourceKey"/> of the PDF stored at <paramref name="pdfPath"/>.
    /// </summary>
    /// <param name="pdfPath">The file system path of the PDF.</param>
    /// <param name="resourceKey">The resource key of the page image to render, of the form <c>page:{n}</c>.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The PNG bytes of the rendered page.</returns>
    /// <remarks>
    /// The PDFtoImage and SkiaSharp APIs used by this method are annotated with the supported platforms of the rendering engine, so this
    /// method must repeat that restriction or the CA1416 analyzer, which is a build error here, flags the call. The attributes must not
    /// be removed, even though Windows, Linux and macOS are the only platforms Lumina runs on.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static byte[] RenderResource(string pdfPath, string resourceKey, CancellationToken cancellationToken)
    {
        // The resource key must name a page of a parseable size: a key that does not start with "page:", does not carry a number,
        // or names a page beyond the page cap (or below the first page) can never be a page of this document, and rejecting it here
        // avoids opening the PDF at all for keys that no parse could have produced.
        if (!resourceKey.StartsWith("page:", StringComparison.Ordinal) || !int.TryParse(resourceKey["page:".Length..], out int pageNumber) || pageNumber < 1 || pageNumber > MAX_PAGES)
            throw new InvalidDataException($"The resource key '{resourceKey}' does not identify a PDF page.");

        // The page is validated against the actual document before any bitmap is allocated: a page number that exceeds the number of
        // pages of the document, or a page whose MediaBox declares dimensions larger than what the render limits allow, would only be
        // discovered by the renderer after it has already allocated a bitmap the size of the page, which a hostile PDF could make huge.
        using (PdfDocument pdfDocument = PdfDocument.Open(pdfPath))
        {
            if (pageNumber > pdfDocument.NumberOfPages)
                throw new InvalidDataException($"The resource key '{resourceKey}' does not identify a PDF page: the PDF contains {pdfDocument.NumberOfPages} pages.");

            Page page = pdfDocument.GetPage(pageNumber);
            if (page.Width > MAX_RENDER_PAGE_DIMENSION || page.Height > MAX_RENDER_PAGE_DIMENSION || page.Width * page.Height > MAX_RENDER_PAGE_AREA)
                throw new InvalidDataException($"The resource key '{resourceKey}' identifies a page that is too large to render ({page.Width:0.#} by {page.Height:0.#} points).");
        }

        using (FileStream pdfStream = File.OpenRead(pdfPath))
        {
            // The page is rendered at its natural size; the height follows the aspect ratio of the page. PdfPig numbers the pages
            // from 1, while PDFtoImage indexes them from 0, so the zero-based index of the rendered page is one less than its 1-based number.
            using (SKBitmap pageBitmap = Conversion.ToImage(pdfStream, page: new Index(pageNumber - 1), leaveOpen: true))
            {
                using (SKData pageImage = pageBitmap.Encode(SKEncodedImageFormat.Png, 100))
                    return pageImage.ToArray();
            }
        }
    }

    /// <summary>
    /// Parses the PDF stored at <paramref name="pdfPath"/> by extracting the text layer of its pages, and returns its normalized reading document.
    /// </summary>
    /// <param name="pdfPath">The file system path of the PDF.</param>
    /// <param name="sectionsDirectory">The directory into which the section files of the PDF are written.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The normalized reading document of the PDF, with a text layer when the PDF has one.</returns>
    private static ReadingDocumentDto ParseAsText(string pdfPath, string sectionsDirectory, CancellationToken cancellationToken)
    {
        List<ReadingSpineItemDto> spine = [];
        bool hasTextContent = false;
        List<ReadingTocEntryDto> tableOfContents = [];
        using (PdfDocument pdfDocument = PdfDocument.Open(pdfPath))
        {
            foreach (Page page in pdfDocument.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (page.Number > MAX_PAGES)
                    break;

                // The location reference of a page is its 1-based page number (PdfPig pages are numbered from 1), which is the only stable identifier a PDF page has, and it doubles as the name of the section file.
                string locationRef = $"page:{page.Number}";
                string fileName = $"{page.Number}.html";
                string pageHtml = BuildPageHtml(page);
                // A page may have no text at all, in which case the HTML is only the empty section wrapper; the text-content
                // detection looks at the paragraphs actually produced, because the wrapper is always present.
                if (pageHtml.Contains("<p>"))
                    hasTextContent = true;
                File.WriteAllText(Path.Combine(sectionsDirectory, fileName), pageHtml, Encoding.UTF8);
                spine.Add(new ReadingSpineItemDto(locationRef, null, $"sections/{fileName}"));
            }

            // The table of contents is read from the document that was already opened for the pages, so the PDF is not opened twice.
            tableOfContents = ReadTableOfContents(pdfDocument);
        }

        // The author and the cover are not extracted in this text-only iteration, and a PDF has no embedded resources to serve, so the resources dictionary is empty.
        return new ReadingDocumentDto(Path.GetFileNameWithoutExtension(pdfPath), null, null, tableOfContents, spine, new Dictionary<string, ReadingResourceInfoDto>(), hasTextContent);
    }

    /// <summary>
    /// Builds the HTML content of a PDF page, wrapping its extracted text into paragraphs.
    /// </summary>
    /// <param name="page">The PDF page whose content is built.</param>
    /// <returns>The HTML content of the PDF page.</returns>
    private static string BuildPageHtml(Page page)
    {
        // The extracted text comes from the untrusted document, so each line is HTML-encoded before it is wrapped, keeping any embedded markup inert until the host sanitizes the whole section anyway;
        // the lines are wrapped in paragraphs so that the page reads like a document, instead of one unbroken wall of text.
        StringBuilder stringBuilder = new();
        stringBuilder.Append("<section>");
        string text = page.Text ?? string.Empty;
        foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            stringBuilder.Append("<p>").Append(WebUtility.HtmlEncode(line)).Append("</p>");
        }
        stringBuilder.Append("</section>");
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Reads the table of contents of the PDF from the bookmarks of the already open document.
    /// </summary>
    /// <param name="pdfDocument">The open PDF document whose bookmarks are read.</param>
    /// <returns>The table of contents of the PDF, or an empty list when the PDF has no usable bookmarks.</returns>
    private static List<ReadingTocEntryDto> ReadTableOfContents(PdfDocument pdfDocument)
    {
        try
        {
            // Many PDFs have no bookmarks at all (for example a book exported from a word processor), in which case the reader simply shows no table of contents instead of failing the whole book.
            if (!pdfDocument.TryGetBookmarks(out Bookmarks? bookmarks) || bookmarks is null || bookmarks.Roots.Count == 0)
                return [];

            return [.. bookmarks.Roots.Select(ToTocEntry)];
        }
        catch (OperationCanceledException)
        {
            // A cancelled parse must not silently succeed with an empty table of contents, so the cancellation is rethrown instead of
            // being swallowed: the request was aborted, not the bookmarks found unreadable.
            throw;
        }
        catch (Exception)
        {
            // The bookmarks of a malformed PDF are best effort, so the table of contents is skipped instead of failing the book; the pages themselves are still readable
            return [];
        }
    }

    /// <summary>
    /// Converts a bookmark of the PDF into a table of contents entry.
    /// </summary>
    /// <param name="bookmark">The bookmark to convert.</param>
    /// <returns>The converted table of contents entry.</returns>
    private static ReadingTocEntryDto ToTocEntry(BookmarkNode bookmark)
    {
        // Only a document bookmark has a destination page; the other bookmark kinds (external, embedded, URI) cannot be navigated inside the reader, so they yield an entry that points nowhere,
        // and the client simply does not render it as navigable.
        int? pageNumber = bookmark is DocumentBookmarkNode documentBookmark ? documentBookmark.PageNumber : null;
        string locationRef = pageNumber is null ? string.Empty : $"page:{pageNumber.Value}";
        // The nested bookmarks of a bookmark become the children of the entry, preserving the hierarchy of the outline.
        List<ReadingTocEntryDto> children = [.. bookmark.Children.Select(ToTocEntry)];
        return new ReadingTocEntryDto(bookmark.Title ?? string.Empty, locationRef, children);
    }
}
