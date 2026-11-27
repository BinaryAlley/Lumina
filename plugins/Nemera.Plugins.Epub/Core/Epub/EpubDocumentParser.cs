#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Plugins.Epub.Common.Models.DTO.Opf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
#endregion

namespace Lumina.Plugins.Epub.Core.Epub;

/// <summary>
/// Parses an EPUB into a normalized reading document, extracting its sections and resources into a working directory.
/// </summary>
/// <remarks>
/// An EPUB is a ZIP archive, so this parser works entirely with archive entry paths (always separated by '/', regardless of
/// the host operating system) and never touches the real file system except for the working directory it writes into.
/// The structure of an EPUB, read from the archive:
/// <list type="bullet">
///   <item>
///     <description>
///     <c>META-INF/container.xml</c> is the entry point. Its rootfile element points to the OPF package file.
///     </description>
///   </item>
///   <item>
///     <description>
///     The OPF package file describes the whole book, with three relevant sections:
///     <list type="bullet">
///       <item>
///         <description>
///         <c>metadata</c>: the title, the author, the language, etc. (in the Dublin Core namespace, prefixed <c>dc</c>).
///         </description>
///       </item>
///       <item>
///         <description>
///         <c>manifest</c>: the list of every file that is part of the publication (chapters, images, fonts, CSS, the
///         navigation document). Each item has an <c>id</c> (unique within the manifest), an <c>href</c> (the path of the
///         file relative to the OPF file), and a <c>media-type</c>.
///         </description>
///       </item>
///       <item>
///         <description>
///         <c>spine</c>: the READING ORDER of the book, named after the spine of a physical book. It is a list of
///         <c>itemref</c> elements, each referencing a manifest item by its <c>id</c>. The spine is literally how the book
///         reads front to back: chapter 1 first, then chapter 2, and so on. The items listed in the spine are the
///         "sections" this reader serves.
///         </description>
///       </item>
///     </list>
///     </description>
///   </item>
///   <item>
///     <description>
///     The table of contents comes from the navigation document (EPUB 3) or the NCX document (EPUB 2).
///     </description>
///   </item>
/// </list>
/// This parser extracts each spine section as an HTML file and each other usable file (images, fonts, CSS) as a resource,
/// and describes the result with the format-agnostic <see cref="ReadingDocumentDto"/> model. The host serves those files.
/// </remarks>
internal static class EpubDocumentParser
{
    // The archive is untrusted input, so every document read from it is bounded, and the total expanded size is capped so
    // that a tiny archive cannot expand into a decompression bomb that exhausts the memory of the host.
    private const long MAX_CONTAINER_FILE_SIZE_BYTES = 1024 * 1024;
    private const long MAX_OPF_FILE_SIZE_BYTES = 5 * 1024 * 1024;
    private const long MAX_NAV_FILE_SIZE_BYTES = 5 * 1024 * 1024;
    private const long MAX_SECTION_FILE_SIZE_BYTES = 10 * 1024 * 1024;
    private const long MAX_RESOURCE_FILE_SIZE_BYTES = 20 * 1024 * 1024;
    private const long MAX_TOTAL_EXPANDED_BYTES = 256 * 1024 * 1024;
    private const int MAX_ARCHIVE_ENTRIES = 20_000;

    private static readonly XNamespace s_opfNamespace = "http://www.idpf.org/2007/opf";
    private static readonly XNamespace s_dcNamespace = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace s_ncxNamespace = "http://www.daisy.org/z3986/2005/ncx/";
    private static readonly XNamespace s_epubNamespace = "http://www.idpf.org/2007/ops";
    private static readonly XNamespace s_xlinkNamespace = "http://www.w3.org/1999/xlink";
    private static readonly XNamespace s_containerNamespace = "urn:oasis:names:tc:opendocument:xmlns:container";

    // The XHTML content documents, the navigation documents, and the package documents of real books declare a DTD (for example
    // "<!DOCTYPE html PUBLIC ...>") and use the named character references that the DTD defines (for example &nbsp;). The parser must
    // never load those external DTDs, because they are untrusted input, so before a document is parsed, its document type declaration
    // is removed and the HTML named character references the DTD would have defined are resolved into the characters they stand for.
    // The XML parser still resolves the five entities defined by XML itself (&amp;, &lt;, &gt;, &quot;, &apos;) and the numeric references.
    private static readonly Dictionary<string, char> s_htmlNamedCharacterReferences = new()
    {
        ["nbsp"] = '\u00A0', ["iexcl"] = '\u00A1', ["cent"] = '\u00A2', ["pound"] = '\u00A3', ["curren"] = '\u00A4', ["yen"] = '\u00A5',
        ["brvbar"] = '\u00A6', ["sect"] = '\u00A7', ["uml"] = '\u00A8', ["copy"] = '\u00A9', ["ordf"] = '\u00AA', ["laquo"] = '\u00AB',
        ["not"] = '\u00AC', ["shy"] = '\u00AD', ["reg"] = '\u00AE', ["macr"] = '\u00AF', ["deg"] = '\u00B0', ["plusmn"] = '\u00B1',
        ["sup2"] = '\u00B2', ["sup3"] = '\u00B3', ["acute"] = '\u00B4', ["micro"] = '\u00B5', ["para"] = '\u00B6', ["middot"] = '\u00B7',
        ["cedil"] = '\u00B8', ["sup1"] = '\u00B9', ["ordm"] = '\u00BA', ["raquo"] = '\u00BB', ["frac14"] = '\u00BC', ["frac12"] = '\u00BD',
        ["frac34"] = '\u00BE', ["iquest"] = '\u00BF', ["Agrave"] = '\u00C0', ["Aacute"] = '\u00C1', ["Acirc"] = '\u00C2', ["Atilde"] = '\u00C3',
        ["Auml"] = '\u00C4', ["Aring"] = '\u00C5', ["AElig"] = '\u00C6', ["Ccedil"] = '\u00C7', ["Egrave"] = '\u00C8', ["Eacute"] = '\u00C9',
        ["Ecirc"] = '\u00CA', ["Euml"] = '\u00CB', ["Igrave"] = '\u00CC', ["Iacute"] = '\u00CD', ["Icirc"] = '\u00CE', ["Iuml"] = '\u00CF',
        ["ETH"] = '\u00D0', ["Ntilde"] = '\u00D1', ["Ograve"] = '\u00D2', ["Oacute"] = '\u00D3', ["Ocirc"] = '\u00D4', ["Otilde"] = '\u00D5',
        ["Ouml"] = '\u00D6', ["times"] = '\u00D7', ["Oslash"] = '\u00D8', ["Ugrave"] = '\u00D9', ["Uacute"] = '\u00DA', ["Ucirc"] = '\u00DB',
        ["Uuml"] = '\u00DC', ["Yacute"] = '\u00DD', ["THORN"] = '\u00DE', ["szlig"] = '\u00DF', ["agrave"] = '\u00E0', ["aacute"] = '\u00E1',
        ["acirc"] = '\u00E2', ["atilde"] = '\u00E3', ["auml"] = '\u00E4', ["aring"] = '\u00E5', ["aelig"] = '\u00E6', ["ccedil"] = '\u00E7',
        ["egrave"] = '\u00E8', ["eacute"] = '\u00E9', ["ecirc"] = '\u00EA', ["euml"] = '\u00EB', ["igrave"] = '\u00EC', ["iacute"] = '\u00ED',
        ["icirc"] = '\u00EE', ["iuml"] = '\u00EF', ["eth"] = '\u00F0', ["ntilde"] = '\u00F1', ["ograve"] = '\u00F2', ["oacute"] = '\u00F3',
        ["ocirc"] = '\u00F4', ["otilde"] = '\u00F5', ["ouml"] = '\u00F6', ["divide"] = '\u00F7', ["oslash"] = '\u00F8', ["ugrave"] = '\u00F9',
        ["uacute"] = '\u00FA', ["ucirc"] = '\u00FB', ["uuml"] = '\u00FC', ["yacute"] = '\u00FD', ["thorn"] = '\u00FE', ["yuml"] = '\u00FF',
        ["OElig"] = '\u0152', ["oelig"] = '\u0153', ["Scaron"] = '\u0160', ["scaron"] = '\u0161', ["Yuml"] = '\u0178', ["fnof"] = '\u0192',
        ["circ"] = '\u02C6', ["tilde"] = '\u02DC', ["ensp"] = '\u2002', ["emsp"] = '\u2003', ["thinsp"] = '\u2009', ["zwnj"] = '\u200C',
        ["zwj"] = '\u200D', ["lrm"] = '\u200E', ["rlm"] = '\u200F', ["ndash"] = '\u2013', ["mdash"] = '\u2014', ["lsquo"] = '\u2018',
        ["rsquo"] = '\u2019', ["sbquo"] = '\u201A', ["ldquo"] = '\u201C', ["rdquo"] = '\u201D', ["bdquo"] = '\u201E', ["dagger"] = '\u2020',
        ["Dagger"] = '\u2021', ["bull"] = '\u2022', ["hellip"] = '\u2026', ["permil"] = '\u2030', ["prime"] = '\u2032', ["Prime"] = '\u2033',
        ["lsaquo"] = '\u2039', ["rsaquo"] = '\u203A', ["oline"] = '\u203E', ["frasl"] = '\u2044', ["euro"] = '\u20AC', ["image"] = '\u2111',
        ["weierp"] = '\u2118', ["real"] = '\u211C', ["trade"] = '\u2122', ["alefsym"] = '\u2135', ["larr"] = '\u2190', ["uarr"] = '\u2191',
        ["rarr"] = '\u2192', ["darr"] = '\u2193', ["harr"] = '\u2194', ["crarr"] = '\u21B5', ["lArr"] = '\u21D0', ["uArr"] = '\u21D1',
        ["rArr"] = '\u21D2', ["dArr"] = '\u21D3', ["hArr"] = '\u21D4', ["forall"] = '\u2200', ["part"] = '\u2202', ["exist"] = '\u2203',
        ["empty"] = '\u2205', ["nabla"] = '\u2207', ["isin"] = '\u2208', ["notin"] = '\u2209', ["ni"] = '\u220B', ["prod"] = '\u220F',
        ["sum"] = '\u2211', ["minus"] = '\u2212', ["lowast"] = '\u2217', ["radic"] = '\u221A', ["prop"] = '\u221D', ["infin"] = '\u221E',
        ["ang"] = '\u2220', ["and"] = '\u2227', ["or"] = '\u2228', ["cap"] = '\u2229', ["cup"] = '\u222A', ["int"] = '\u222B',
        ["there4"] = '\u2234', ["sim"] = '\u223C', ["cong"] = '\u2245', ["asymp"] = '\u2248', ["ne"] = '\u2260', ["equiv"] = '\u2261',
        ["le"] = '\u2264', ["ge"] = '\u2265', ["sub"] = '\u2282', ["sup"] = '\u2283', ["nsub"] = '\u2284', ["sube"] = '\u2286',
        ["supe"] = '\u2287', ["oplus"] = '\u2295', ["otimes"] = '\u2297', ["perp"] = '\u22A5', ["sdot"] = '\u22C5', ["lceil"] = '\u2308',
        ["rceil"] = '\u2309', ["lfloor"] = '\u230A', ["rfloor"] = '\u230B', ["lang"] = '\u2329', ["rang"] = '\u232A', ["loz"] = '\u25CA',
        ["spades"] = '\u2660', ["clubs"] = '\u2663', ["hearts"] = '\u2665', ["diams"] = '\u2666'
    };
    private static readonly Regex s_namedCharacterReferencePattern = new("&([A-Za-z][A-Za-z0-9]*);", RegexOptions.Compiled);

    /// <summary>
    /// Parses the EPUB stored at <paramref name="epubPath"/>, extracting its sections and resources into <paramref name="workingDirectory"/>, and returns its normalized reading document.
    /// </summary>
    /// <param name="epubPath">The file system path of the EPUB.</param>
    /// <param name="workingDirectory">The directory into which the sections and resources of the EPUB are extracted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The normalized reading document of the EPUB.</returns>
    public static ReadingDocumentDto Parse(string epubPath, string workingDirectory, CancellationToken cancellationToken)
    {
        // The EPUB is opened as a ZIP archive.
        using FileStream fileStream = File.OpenRead(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Read);
        ValidateArchive(zipArchive);

        // The declared sizes are not trustworthy, so the total-expanded cap is enforced on the bytes actually read: every entry read below is streamed through
        // ReadEntry, which adds every byte it returns to this running total and rejects the archive the moment the cap is exceeded (see ReadEntry).
        long totalExpandedBytes = 0;

        // The container document points to the OPF package file, the file that describes the whole book;
        // without it, there is no way to know what the book contains, so there is nothing left to parse.
        string? opfPath = ReadContainerPath(zipArchive, ref totalExpandedBytes, cancellationToken) ?? throw new InvalidDataException("The EPUB contains no OPF file.");

        // The manifest hrefs are relative to the directory of the OPF file, so that directory is kept around to resolve them.
        string opfDirectory = GetDirectory(opfPath);
        OpfDocumentDto opfDocument = ReadOpf(zipArchive, opfPath, ref totalExpandedBytes, cancellationToken);

        // The spine is the reading order of the book. An EPUB with an empty spine declares no readable content at all - there is nothing to present to the reader.
        if (opfDocument.SpineIds.Count == 0)
            throw new InvalidDataException("The EPUB contains no reading sections.");

        // Build a lookup that maps every reachable archive file to the opaque value its references should be rewritten to.
        // A section's HTML references other files of the book by their archive path, for example, <img src="images/cover.png">, but the client could be remote, and cannot reach into the ZIP.
        // So, before a section is served, every such reference must be replaced by a marker (data-lumina-resource) that the host turns into a resource endpoint URL.
        // This lookup is what makes that rewrite possible: given the archive path a reference points to, it answers which marker to use.
        Dictionary<string, string> targetByResolvedPath = [];
        HashSet<string> spineItemIds = [.. opfDocument.SpineIds];
        foreach (OpfManifestItemDto epubManifestItem in opfDocument.Items)
        {
            // Resolve the href of the manifest item into its actual archive entry path; null means the href is broken or escapes the archive (for example, an absolute path or a path with ".." above the root),
            // which is _ONE_ bad manifest item, so it is skipped and the rest of the book is still parsed.
            string? entryPath = ResolveEntryPath(opfDirectory, epubManifestItem.Href);
            if (entryPath is null)
                continue;

            // A reference to a spine section is rewritten to the section's location reference (the manifest item Id), so the client can navigate to it; a reference to any other extractable resource is rewritten to the resource
            // key, so the client can load it.
            if (spineItemIds.Contains(epubManifestItem.Id))
                targetByResolvedPath[entryPath] = epubManifestItem.Id;
            else if (IsResourceMediaType(epubManifestItem.MediaType))
                targetByResolvedPath[entryPath] = CreateResourceKey(epubManifestItem.Href);
        }

        // The rest of the extraction follows the same order as the document model: resources first, then the table of contents, then the sections, so that every stage has what the next one needs.
        Dictionary<string, ReadingResourceInfoDto> resources = ExtractResources(zipArchive, workingDirectory, opfDirectory, opfDocument, spineItemIds, ref totalExpandedBytes, cancellationToken);

        (List<ReadingTocEntryDto> tableOfContents, Dictionary<string, string> titlesByItemId) = BuildTableOfContents(zipArchive, opfDocument, opfDirectory, targetByResolvedPath, ref totalExpandedBytes, cancellationToken);

        List<ReadingSpineItemDto> spine = ExtractSections(zipArchive, workingDirectory, opfDirectory, opfDocument, targetByResolvedPath, titlesByItemId, ref totalExpandedBytes, cancellationToken);

        // The cover is the manifest item flagged as the cover image (see ReadOpf), exposed to the client as a resource key so the client can fetch it through the resource endpoint like any other image of the book.
        string? coverResourceKey = null;
        if (opfDocument.CoverItemId is not null && opfDocument.ItemsById.TryGetValue(opfDocument.CoverItemId, out OpfManifestItemDto? coverItem) && resources.TryGetValue(CreateResourceKey(coverItem.Href), out _))
            coverResourceKey = CreateResourceKey(coverItem.Href);

        // Fall back to the file name when the package declares no title, so that the book always has a usable title.
        string title = !string.IsNullOrWhiteSpace(opfDocument.Title) ? opfDocument.Title.Trim() : Path.GetFileNameWithoutExtension(epubPath);
        return new ReadingDocumentDto(title, opfDocument.Author, coverResourceKey, tableOfContents, spine, resources, HasTextContent: true);
    }

    /// <summary>
    /// Validates the archive bounds, so that a decompression bomb cannot exhaust the memory of the host.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive to validate.</param>
    private static void ValidateArchive(ZipArchive zipArchive)
    {
        // A decompression bomb is a small ZIP whose entries expand into a huge amount of data; a simple way to defend against it is to bound how many entries the archive may declare and how much uncompressed data it may claim.
        if (zipArchive.Entries.Count > MAX_ARCHIVE_ENTRIES)
            throw new InvalidDataException("The EPUB archive contains too many entries.");

        // The declared uncompressed sizes are not trustworthy, so this sum only bounds the worst case before any single entry is read; it is a cheap first guard,
        // not the real defense. The real cap is enforced on the bytes actually read while every entry is streamed (see ReadEntry), because a hostile archive could
        // declare small sizes yet expand into far more data than the parser is willing to extract.
        long totalExpandedBytes = 0;
        foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
        {
            totalExpandedBytes += zipArchiveEntry.Length;
            if (totalExpandedBytes > MAX_TOTAL_EXPANDED_BYTES)
                throw new InvalidDataException("The expanded EPUB archive is too large.");
        }
    }

    /// <summary>
    /// Reads the path of the OPF file from the container document of the EPUB.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The path of the OPF file, or <see langword="null"/> when it could not be read.</returns>
    private static string? ReadContainerPath(ZipArchive zipArchive, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        // The container document lives at a fixed, mandatory location inside the archive; if it is missing, the EPUB is structurally invalid and cannot be opened.
        ZipArchiveEntry? containerEntry = zipArchive.GetEntry("META-INF/container.xml");
        if (containerEntry is null)
            return null;

        XDocument container;
        try
        {
            container = ReadXmlEntry(zipArchive, "META-INF/container.xml", MAX_CONTAINER_FILE_SIZE_BYTES, ref totalExpandedBytes, cancellationToken);
        }
        catch (XmlException)
        {
            // A malformed container document means the book cannot be opened, but it must not crash the host, so it is reported as "no OPF file" instead of propagating the parse error.
            return null;
        }

        // The rootfile element holds the path of the OPF package file in its "full-path" attribute; the container uses its own XML namespace, which is unrelated to the OPF namespace used by the rest of the book.
        return container.Descendants(s_containerNamespace + "rootfile").FirstOrDefault()?.Attribute("full-path")?.Value;
    }

    /// <summary>
    /// Reads and parses the OPF document of the EPUB.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="opfPath">The path of the OPF file.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The parsed OPF document.</returns>
    private static OpfDocumentDto ReadOpf(ZipArchive zipArchive, string opfPath, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        XDocument xDocument = ReadXmlEntry(zipArchive, opfPath, MAX_OPF_FILE_SIZE_BYTES, ref totalExpandedBytes, cancellationToken);
        XElement? package = xDocument.Root;

        // The metadata holds the title and the author of the book, in the Dublin Core namespace.
        OpfDocumentDto opfDocument = new()
        {
            Title = package?.Element(s_opfNamespace + "metadata")?.Element(s_dcNamespace + "title")?.Value,
            Author = package?.Element(s_opfNamespace + "metadata")?.Element(s_dcNamespace + "creator")?.Value
        };

        // The navigation document and the cover image are identified by the properties of their manifest items; both are
        // optional (an EPUB may have no table of contents and no cover), and the first item carrying the property wins.
        string? navItemId = null;
        string? coverItemId = null;
        foreach (XElement itemElement in package?.Element(s_opfNamespace + "manifest")?.Elements(s_opfNamespace + "item") ?? [])
        {
            string? id = itemElement.Attribute("id")?.Value;
            string? href = itemElement.Attribute("href")?.Value;
            string? mediaType = itemElement.Attribute("media-type")?.Value;
            // An item without an id or an href cannot be referenced by the spine or by the sections, so it is useless and skipped, like any other single broken manifest item.
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(href))
                continue;

            OpfManifestItemDto opfManifestItem = new()
            {
                Id = id,
                Href = href.Trim(),
                MediaType = mediaType ?? string.Empty
            };
            opfDocument.ItemsById[id] = opfManifestItem;
            opfDocument.Items.Add(opfManifestItem);

            string? properties = itemElement.Attribute("properties")?.Value;
            if (properties is not null && properties.Split(' ').Any(property => string.Equals(property, "nav", StringComparison.OrdinalIgnoreCase)))
                navItemId ??= id;
            if (properties is not null && properties.Split(' ').Any(property => string.Equals(property, "cover-image", StringComparison.OrdinalIgnoreCase)))
                coverItemId ??= id;
        }

        // The spine lists the reading order of the sections, as references to manifest item Ids (see the class remarks).
        // An item that is not listed in the spine is not a section, even when it carries textual content (for example, the navigation document), which is why the sections are derived from the spine and not from the manifest.
        // The lookup against itemsById drops the itemrefs that reference an unknown Id, since they are broken references.
        opfDocument.SpineIds.AddRange(package?.Element(s_opfNamespace + "spine")?.Elements(s_opfNamespace + "itemref")
            .Select(itemRef => itemRef.Attribute("idref")?.Value)
            .Where(idref => idref is not null)
            .Select(idref => idref!)
            .Where(id => opfDocument.ItemsById.ContainsKey(id)) ?? []);

        // EPUB 2 declares the cover with a meta element referencing the manifest item of the cover image, instead of the cover-image property of the item itself, so both declarations are honored and whichever one exists wins.
        foreach (XElement metaElement in package?.Element(s_opfNamespace + "metadata")?.Elements(s_opfNamespace + "meta") ?? [])
        {
            if (string.Equals(metaElement.Attribute("name")?.Value, "cover", StringComparison.OrdinalIgnoreCase))
            {
                string? content = metaElement.Attribute("content")?.Value;
                if (content is not null && opfDocument.ItemsById.ContainsKey(content))
                    coverItemId ??= content;
            }
        }

        // The NCX document, used as the fallback table of contents of EPUB 2, is referenced by the "toc" attribute of the spine, and is used only when the book has no EPUB 3 navigation document (see BuildTableOfContents).
        opfDocument.NavItemId = navItemId;
        opfDocument.CoverItemId = coverItemId;
        opfDocument.NcxItemId = package?.Element(s_opfNamespace + "spine")?.Attribute("toc")?.Value;

        return opfDocument;
    }

    /// <summary>
    /// Extracts the resources of the EPUB into the resources directory of the working directory.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="workingDirectory">The directory into which the resources are extracted.</param>
    /// <param name="opfDirectory">The directory of the OPF file, used to resolve the manifest hrefs.</param>
    /// <param name="opfDocument">The parsed OPF document.</param>
    /// <param name="spineItemIds">The Ids of the manifest items that are reading sections, which are not extracted as resources.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The extracted resources, keyed by their resource key.</returns>
    private static Dictionary<string, ReadingResourceInfoDto> ExtractResources(ZipArchive zipArchive, string workingDirectory, string opfDirectory, OpfDocumentDto opfDocument, HashSet<string> spineItemIds, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        Dictionary<string, ReadingResourceInfoDto> resources = [];
        foreach (OpfManifestItemDto opfManifestItem in opfDocument.Items)
        {
            // The spine sections are extracted separately as readable sections, so they are skipped here; everything else that is a usable media file (image, font, CSS, audio, video) is extracted as a resource,
            // and the rest of the manifest (for example the navigation document) is not extracted at all, because nothing references it.
            if (spineItemIds.Contains(opfManifestItem.Id) || !IsResourceMediaType(opfManifestItem.MediaType))
                continue;

            string? entryPath = ResolveEntryPath(opfDirectory, opfManifestItem.Href);
            if (entryPath is null)
                continue;
            ZipArchiveEntry? zipArchiveEntry = zipArchive.GetEntry(entryPath);
            if (zipArchiveEntry is null)
                continue;

            // The resource key is derived from the manifest href, so the same file referenced from several sections maps to one extracted file, and the key doubles as a safe, opaque file name that cannot contain path characters.
            string key = CreateResourceKey(opfManifestItem.Href);
            if (resources.ContainsKey(key))
                continue;

            byte[] data = ReadEntry(zipArchiveEntry, MAX_RESOURCE_FILE_SIZE_BYTES, ref totalExpandedBytes, cancellationToken);
            string destinationDirectory = Path.Combine(workingDirectory, "resources");
            Directory.CreateDirectory(destinationDirectory);
            File.WriteAllBytes(Path.Combine(destinationDirectory, key), data);
            // The media type declared by the manifest is kept, so that the host can serve the resource with the right Content-Type; a missing media type falls back to a generic binary type.
            resources[key] = new ReadingResourceInfoDto($"resources/{key}", string.IsNullOrWhiteSpace(opfManifestItem.MediaType) ? "application/octet-stream" : opfManifestItem.MediaType);
        }
        return resources;
    }

    /// <summary>
    /// Extracts the reading sections of the EPUB into the sections directory of the working directory.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="workingDirectory">The directory into which the sections are extracted.</param>
    /// <param name="opfDirectory">The directory of the OPF file, used to resolve the manifest hrefs.</param>
    /// <param name="opfDocument">The parsed OPF document.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="titlesByItemId">The titles of the reading sections, keyed by the Id of their manifest item.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The extracted reading sections of the EPUB.</returns>
    private static List<ReadingSpineItemDto> ExtractSections(ZipArchive zipArchive, string workingDirectory, string opfDirectory, OpfDocumentDto opfDocument, Dictionary<string, string> targetByResolvedPath, Dictionary<string, string> titlesByItemId, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        List<ReadingSpineItemDto> spine = [];
        string sectionsDirectory = Path.Combine(workingDirectory, "sections");
        Directory.CreateDirectory(sectionsDirectory);
        for (int index = 0; index < opfDocument.SpineIds.Count; index++)
        {
            string itemId = opfDocument.SpineIds[index];
            OpfManifestItemDto opfManifestItem = opfDocument.ItemsById[itemId];

            string? entryPath = ResolveEntryPath(opfDirectory, opfManifestItem.Href);
            if (entryPath is null)
                continue;
            ZipArchiveEntry? zipArchiveEntry = zipArchive.GetEntry(entryPath);
            if (zipArchiveEntry is null)
                continue;

            byte[] data = ReadEntry(zipArchiveEntry, MAX_SECTION_FILE_SIZE_BYTES, ref totalExpandedBytes, cancellationToken);
            string content = RewriteSectionContent(data, entryPath, targetByResolvedPath, cancellationToken);

            // The section file name is the spine position, which is stable, while the location reference exposed to the client is the manifest item Id,
            // which is the identifier shared with the table of contents entries; keeping the two separate means the file name never leaks to the client.
            string sectionFileName = $"{index}.html";
            File.WriteAllText(Path.Combine(sectionsDirectory, sectionFileName), content, Encoding.UTF8);
            spine.Add(new ReadingSpineItemDto(itemId, titlesByItemId.GetValueOrDefault(itemId), $"sections/{sectionFileName}"));
        }
        return spine;
    }

    /// <summary>
    /// Rewrites the resource references of a reading section into the resource marker attribute, so that the host can
    /// rewrite them to the resource endpoint when serving the section.
    /// </summary>
    /// <param name="data">The raw content of the reading section.</param>
    /// <param name="sectionEntryPath">The archive path of the reading section, used to resolve its references.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The rewritten content of the reading section.</returns>
    private static string RewriteSectionContent(byte[] data, string sectionEntryPath, Dictionary<string, string> targetByResolvedPath, CancellationToken cancellationToken)
    {
        // The section is parsed as XML so that its references can be rewritten in a structured, attribute-by-attribute way.
        XDocument xDocument;
        try
        {
            // A section of a real book typically declares a DTD (for example "<!DOCTYPE html PUBLIC ...>") and uses the named character
            // references that the DTD defines (for example &nbsp;); the external DTD is never loaded because it is untrusted, so the content
            // is normalized first - the document type declaration is removed and the HTML named character references are resolved - and only
            // then parsed as XML.
            string normalizedContent = NormalizeDocumentText(data);
            XmlReaderSettings xmlReaderSettings = new()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = MAX_SECTION_FILE_SIZE_BYTES,
                MaxCharactersFromEntities = 0
            };
            using StringReader stringReader = new(normalizedContent);
            using XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings);
            xDocument = XDocument.Load(xmlReader, LoadOptions.None);
        }
        catch (XmlException)
        {
            // A section that is not well formed XML cannot have its references rewritten in a structured way, so it is served "as-is",
            // with its internal references unresolved; the host still sanitizes it before serving, so the section stays safe even when its images cannot load.
            return Encoding.UTF8.GetString(data);
        }

        string sectionDirectory = GetDirectory(sectionEntryPath);

        // xDocument is the XML tree of the section: an element can contain child elements, which contain their own children, and so on, like a family tree. Descendants() yields EVERY element nested at any depth below the root -
        // the direct children, the grandchildren, the great-grandchildren, etc. - unlike Elements(), which yields only the direct children. The whole tree must be walked, because the images and links of a chapter can be nested
        // arbitrarily deep inside divs, paragraphs, tables, etc., and every one of their references needs the same rewrite.
        foreach (XElement element in xDocument.Descendants())
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ToList() takes a snapshot of the attributes before iterating, because the loop removes the rewritten attributes below (attribute.Remove()); mutating a collection while enumerating it would throw an exception.
            // An attribute is a name="value" pair written on the opening tag of the element (for example class, id, style).
            foreach (XAttribute attribute in element.Attributes().ToList())
            {
                // Most attributes cannot point at another file (class, id, style, ...), so they are skipped right away; only the ones that carry a reference (src, href, poster, xlink:href) are candidates for the rewrite.
                if (!IsResourceReferenceAttribute(attribute))
                    continue;

                // References that cannot point inside the archive are left untouched: same-page anchors (starting with "#"), inline data URIs, and absolute web references, all keep their original value,
                // and the sanitizer strips the dangerous ones before the section is served.
                string? reference = attribute.Value?.Trim();
                if (string.IsNullOrWhiteSpace(reference))
                    continue;
                if (reference.StartsWith('#') || reference.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                    || reference.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || reference.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    || reference.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) || reference.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Spliit the fragment off the path, resolve the path against the directory of the section, and only rewrite references that point at an extracted
                // manifest item; a broken reference (a path that does not exist in the manifest) is left alone, instead of being rewritten to a marker that leads nowhere.
                string path = reference.Contains('#') ? reference[..reference.IndexOf('#')] : reference;
                string? resolvedPath = ResolveEntryPath(sectionDirectory, path);
                if (resolvedPath is null || !targetByResolvedPath.TryGetValue(resolvedPath, out string? target))
                    continue;

                // The original attribute is removed and replaced by the resource marker, so that the browser does not attempt to load the internal archive path,
                // which it cannot reach; the client resolves the marker to the resource endpoint instead.
                element.SetAttributeValue("data-lumina-resource", target);
                attribute.Remove();
            }
        }

        return xDocument.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Determines whether <paramref name="attribute"/> is an attribute carrying a resource reference.
    /// </summary>
    /// <param name="attribute">The attribute to check.</param>
    /// <returns><see langword="true"/> when the attribute carries a resource reference, <see langword="false"/> otherwise.</returns>
    private static bool IsResourceReferenceAttribute(XAttribute attribute)
    {
        // These are the attributes in which a section can point at another file of the book; xlink:href is used by SVG, and the local name is compared, so that the check works regardless of the XML namespace of the element.
        string localName = attribute.Name.LocalName;
        return localName is "src" or "poster" or "href" || attribute.Name == s_xlinkNamespace + "href";
    }

    /// <summary>
    /// Builds the table of contents of the EPUB, from its navigation document or its NCX document.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="opfDocument">The parsed OPF document.</param>
    /// <param name="opfDirectory">The directory of the OPF file, used to resolve the manifest hrefs.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The table of contents entries, and the titles of the reading sections keyed by the Id of their manifest item.</returns>
    private static (List<ReadingTocEntryDto> entries, Dictionary<string, string> titlesByItemId) BuildTableOfContents(ZipArchive zipArchive, OpfDocumentDto opfDocument, string opfDirectory, Dictionary<string, string> targetByResolvedPath, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        Dictionary<string, string> titlesByItemId = [];
        List<ReadingTocEntryDto> entries = [];

        // EPUB 3 carries its table of contents in the navigation document; EPUB 2 falls back to the NCX document, and only when the EPUB 3 one produced no entries, so that a book carrying both prefers the newer format.
        if (opfDocument.NavItemId is not null && opfDocument.ItemsById.TryGetValue(opfDocument.NavItemId, out OpfManifestItemDto? navItem))
        {
            string? navPath = ResolveEntryPath(opfDirectory, navItem.Href);
            if (navPath is not null)
                entries = ReadNavigationDocument(zipArchive, navPath, targetByResolvedPath, titlesByItemId, ref totalExpandedBytes, cancellationToken);
        }

        if (entries.Count == 0 && opfDocument.NcxItemId is not null && opfDocument.ItemsById.TryGetValue(opfDocument.NcxItemId, out OpfManifestItemDto? ncxItem))
        {
            string? ncxPath = ResolveEntryPath(opfDirectory, ncxItem.Href);
            if (ncxPath is not null)
                entries = ReadNcxDocument(zipArchive, ncxPath, targetByResolvedPath, titlesByItemId, ref totalExpandedBytes, cancellationToken);
        }

        return (entries, titlesByItemId);
    }

    /// <summary>
    /// Reads the table of contents from the navigation document of an EPUB 3.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="navPath">The archive path of the navigation document.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="titlesByItemId">The titles of the reading sections, keyed by the Id of their manifest item.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The table of contents entries.</returns>
    private static List<ReadingTocEntryDto> ReadNavigationDocument(ZipArchive zipArchive, string navPath, Dictionary<string, string> targetByResolvedPath, Dictionary<string, string> titlesByItemId, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        XDocument xDocument;
        try
        {
            xDocument = ReadXmlEntry(zipArchive, navPath, MAX_NAV_FILE_SIZE_BYTES, ref totalExpandedBytes, cancellationToken);
        }
        catch (XmlException)
        {
            return [];
        }

        // The table of contents is the nav element declared with the epub:type "toc"; a navigation document may carry other nav elements (for example the page list), which are ignored because they are not the table of contents.
        XElement? navElement = xDocument.Descendants()
            .FirstOrDefault(element => string.Equals(element.Name.LocalName, "nav", StringComparison.OrdinalIgnoreCase)
                && element.Attributes().Any(attribute => string.Equals(attribute.Name.LocalName, "type", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(attribute.Value, "toc", StringComparison.OrdinalIgnoreCase)));
        if (navElement is null)
            return [];

        // The entries of the table of contents are the list items of that nav element; the nested lists inside a list item become its children, which preserves the hierarchy of the table of contents.
        return [.. navElement.Descendants()
            .Where(element => string.Equals(element.Name.LocalName, "li", StringComparison.OrdinalIgnoreCase))
            .Select(element => ParseNavListItem(element, navPath, targetByResolvedPath, titlesByItemId))];
    }

    /// <summary>
    /// Parses a list item of the navigation document into a table of contents entry.
    /// </summary>
    /// <param name="listItem">The list item element to parse.</param>
    /// <param name="navPath">The archive path of the navigation document.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="titlesByItemId">The titles of the reading sections, keyed by the Id of their manifest item.</param>
    /// <returns>The parsed table of contents entry.</returns>
    private static ReadingTocEntryDto ParseNavListItem(XElement listItem, string navPath, Dictionary<string, string> targetByResolvedPath, Dictionary<string, string> titlesByItemId)
    {
        // The label of the entry is the text of its anchor, and its target is the section the anchor points to.
        XElement? anchor = listItem.Elements().FirstOrDefault(element => string.Equals(element.Name.LocalName, "a", StringComparison.OrdinalIgnoreCase));
        string label = string.IsNullOrWhiteSpace(anchor?.Value) ? string.Empty : anchor.Value.Trim();
        string locationRef = GetLocationRef(anchor?.Attribute("href")?.Value, navPath, targetByResolvedPath, titlesByItemId, label);

        // The nested lists of the list item become the children of the entry, so the hierarchy of the table of contents is preserved; a list item that carries no anchor yields an entry whose label and location reference
        // are both empty, which the client simply does not render as a navigable entry.
        List<ReadingTocEntryDto> children = [.. listItem.Elements()
            .Where(element => string.Equals(element.Name.LocalName, "ol", StringComparison.OrdinalIgnoreCase))
            .SelectMany(element => element.Elements().Where(child => string.Equals(child.Name.LocalName, "li", StringComparison.OrdinalIgnoreCase)))
            .Select(child => ParseNavListItem(child, navPath, targetByResolvedPath, titlesByItemId))];

        return new ReadingTocEntryDto(label, locationRef, children);
    }

    /// <summary>
    /// Reads the table of contents from the NCX document of an EPUB 2.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="ncxPath">The archive path of the NCX document.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="titlesByItemId">The titles of the reading sections, keyed by the Id of their manifest item.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The table of contents entries.</returns>
    private static List<ReadingTocEntryDto> ReadNcxDocument(ZipArchive zipArchive, string ncxPath, Dictionary<string, string> targetByResolvedPath, Dictionary<string, string> titlesByItemId, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        XDocument xDocument;
        try
        {
            xDocument = ReadXmlEntry(zipArchive, ncxPath, MAX_NAV_FILE_SIZE_BYTES, ref totalExpandedBytes, cancellationToken);
        }
        catch (XmlException)
        {
            return [];
        }

        // The table of contents of an NCX document lives in its navMap element, as a tree of nested navPoint elements.
        return [.. xDocument.Descendants(s_ncxNamespace + "navMap")
            .SelectMany(element => element.Elements(s_ncxNamespace + "navPoint"))
            .Select(element => ParseNcxNavPoint(element, ncxPath, targetByResolvedPath, titlesByItemId))];
    }

    /// <summary>
    /// Parses a navigation point of the NCX document into a table of contents entry.
    /// </summary>
    /// <param name="navigationPoint">The navigation point element to parse.</param>
    /// <param name="ncxPath">The archive path of the NCX document.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="titlesByItemId">The titles of the reading sections, keyed by the Id of their manifest item.</param>
    /// <returns>The parsed table of contents entry.</returns>
    private static ReadingTocEntryDto ParseNcxNavPoint(XElement navigationPoint, string ncxPath, Dictionary<string, string> targetByResolvedPath, Dictionary<string, string> titlesByItemId)
    {
        // The label of the entry is the text of its navLabel, and its target is the src of its content element; nested navigation points become the children of the entry, mirroring the hierarchy of the navigation document.
        string label = string.IsNullOrWhiteSpace(navigationPoint.Element(s_ncxNamespace + "navLabel")?.Element(s_ncxNamespace + "text")?.Value)
            ? string.Empty
            : navigationPoint.Element(s_ncxNamespace + "navLabel")!.Element(s_ncxNamespace + "text")!.Value.Trim();
        string? href = navigationPoint.Element(s_ncxNamespace + "content")?.Attribute("src")?.Value;
        string locationRef = GetLocationRef(href, ncxPath, targetByResolvedPath, titlesByItemId, label);

        List<ReadingTocEntryDto> children = [.. navigationPoint.Elements(s_ncxNamespace + "navPoint")
            .Select(child => ParseNcxNavPoint(child, ncxPath, targetByResolvedPath, titlesByItemId))];

        return new ReadingTocEntryDto(label, locationRef, children);
    }

    /// <summary>
    /// Resolves the location reference of a table of contents reference, and records the title of its target section.
    /// </summary>
    /// <param name="href">The href of the table of contents reference.</param>
    /// <param name="documentPath">The archive path of the document containing the reference.</param>
    /// <param name="targetByResolvedPath">The target of every resolvable manifest item, keyed by its resolved archive path.</param>
    /// <param name="titlesByItemId">The titles of the reading sections, keyed by the Id of their manifest item.</param>
    /// <param name="label">The label of the table of contents entry.</param>
    /// <returns>The location reference of the target section, or an empty string when it could not be resolved.</returns>
    private static string GetLocationRef(string? href, string documentPath, Dictionary<string, string> targetByResolvedPath, Dictionary<string, string> titlesByItemId, string label)
    {
        if (string.IsNullOrWhiteSpace(href))
            return string.Empty;

        // A table of contents reference may carry a same-page fragment, which is dropped because the reader navigates at the section level, not at a position inside a section.
        string path = href.Contains('#') ? href[..href.IndexOf('#')] : href;
        string? resolvedPath = ResolveEntryPath(GetDirectory(documentPath), path);
        // A reference that cannot be resolved to a known section yields an entry that points nowhere, so that one broken table of contents entry does not drop the whole table of contents.
        if (resolvedPath is null || !targetByResolvedPath.TryGetValue(resolvedPath, out string? target))
            return string.Empty;

        // The label of the first table of contents entry pointing at a section becomes its title, so that the spine items show the chapter names instead of being untitled;
        // TryAdd keeps the first label seen, since it is the one the table of contents lists first.
        if (!string.IsNullOrWhiteSpace(label))
            titlesByItemId.TryAdd(target, label);
        return target;
    }

    /// <summary>
    /// Determines whether the provided media type identifies a resource that is extracted from the EPUB.
    /// </summary>
    /// <param name="mediaType">The media type to check.</param>
    /// <returns><see langword="true"/> when the media type identifies an extractable resource, <see langword="false"/> otherwise.</returns>
    private static bool IsResourceMediaType(string mediaType)
    {
        if (mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return true;
        if (mediaType.StartsWith("font/", StringComparison.OrdinalIgnoreCase))
            return true;
        if (mediaType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) || mediaType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return true;
        // The font media types do not share a common prefix, so the common ones are listed explicitly; everything else in the manifest (for example the navigation document or a script) is not a resource the reader serves.
        return mediaType is "text/css"
            or "application/font-woff" or "application/font-woff2" or "application/vnd.ms-opentype"
            or "application/x-font-ttf" or "application/x-font-opentype";
    }

    /// <summary>
    /// Resolves a manifest href against the <paramref name="baseDirectory"/>, rejecting paths that escape the archive.
    /// </summary>
    /// <param name="baseDirectory">The directory against which the href is resolved.</param>
    /// <param name="href">The href to resolve.</param>
    /// <returns>The normalized archive path, or <see langword="null"/> when it escapes the archive.</returns>
    private static string? ResolveEntryPath(string baseDirectory, string href)
    {
        // The paths are ZIP archive entry paths, which the ZIP spec always separates with '/', so backslashes (which a hostile EPUB could contain) are normalized to '/'
        // and the segments are split on '/' regardless of the host OS. This is why the whole method must NOT use Path.DirectorySeparatorChar or Path.Combine: an EPUB read
        // on Windows still has '/' inside its archive.
        string normalized = href.Replace('\\', '/').Trim();
        // An empty, absolute, or NUL-containing path can never be a legitimate archive entry: an absolute path would point outside the archive, and a NUL byte is a path injection, so both are rejected.
        if (normalized.Length == 0 || normalized.StartsWith('/') || normalized.Contains('\0'))
            return null;

        // The href is relative to the base directory, so they are joined, and then the "." and ".." segments are resolved by hand; a ".." that would walk above the root
        // of the archive would escape it, so it is rejected (a path traversal attempt), which is why the method returns null instead of an entry path.
        string combined = string.IsNullOrEmpty(baseDirectory) ? normalized : $"{baseDirectory}/{normalized}";
        string[] segments = combined.Split('/');
        Stack<string> stack = [];
        foreach (string segment in segments)
        {
            if (segment is "" or ".")
                continue;
            if (segment == "..")
            {
                if (stack.Count == 0)
                    return null;
                stack.Pop();
                continue;
            }
            stack.Push(segment);
        }

        return string.Join('/', stack.Reverse());
    }

    /// <summary>
    /// Gets the directory part of an archive path.
    /// </summary>
    /// <param name="path">The archive path.</param>
    /// <returns>The directory of the archive path.</returns>
    private static string GetDirectory(string path)
    {
        // The paths are ZIP archive entry paths, which the ZIP spec always separates with '/' regardless of the host OS, so this must NOT use Path.DirectorySeparatorChar: an EPUB read on Windows still has '/' inside its archive.
        int lastSlash = path.LastIndexOf('/');
        return lastSlash < 0 ? string.Empty : path[..lastSlash];
    }

    /// <summary>
    /// Creates the opaque resource key of a manifest href.
    /// </summary>
    /// <param name="href">The manifest href.</param>
    /// <returns>The resource key of the href.</returns>
    private static string CreateResourceKey(string href)
    {
        // Hashing the href yields a stable key per file, regardless of the characters it contains, so the same file referenced from several sections always maps to the
        // same extracted file; and a hex digest is a safe file name that cannot contain path characters, so it is used both as the extracted file name and as the resource
        // key the client sends back to the resource endpoint.
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(href)))[..32].ToLowerInvariant();
    }

    /// <summary>
    /// Converts the raw content of an XML or XHTML document of the archive into text that the XML parser can load: the document type
    /// declaration is removed, because the external DTD it references is untrusted and must never be loaded, and the HTML named character
    /// references that the declaration would have defined are resolved into the characters they stand for (see the field documentation).
    /// </summary>
    /// <param name="data">The raw content of the document.</param>
    /// <returns>The content, normalized for XML parsing.</returns>
    private static string NormalizeDocumentText(byte[] data)
    {
        string text = DecodeDocumentText(data);
        if (text.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) >= 0)
            text = RemoveDocumentTypeDeclaration(text);
        if (text.IndexOf('&') >= 0)
            text = ResolveNamedCharacterReferences(text);
        return text;
    }

    /// <summary>
    /// Decodes the raw content of an XML or XHTML document of the archive into text, honoring its byte order mark.
    /// </summary>
    /// <param name="data">The raw content of the document.</param>
    /// <returns>The decoded text of the document, without its byte order mark.</returns>
    private static string DecodeDocumentText(byte[] data)
    {
        // EPUB mandates UTF-8 or UTF-16, so the byte order mark is the reliable way to tell the two apart; a document without a mark is UTF-8.
        if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
            return Encoding.Unicode.GetString(data, 2, data.Length - 2);
        if (data.Length >= 2 && data[0] == 0xFE && data[1] == 0xFF)
            return Encoding.BigEndianUnicode.GetString(data, 2, data.Length - 2);
        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            return Encoding.UTF8.GetString(data, 3, data.Length - 3);
        return Encoding.UTF8.GetString(data);
    }

    /// <summary>
    /// Removes every document type declaration of an XML or XHTML document, including any internal subset it carries, so that the XML
    /// parser never encounters the external DTD the declaration references.
    /// </summary>
    /// <param name="text">The text of the document.</param>
    /// <returns>The text of the document without its document type declarations.</returns>
    private static string RemoveDocumentTypeDeclaration(string text)
    {
        // The declaration spans from "<!DOCTYPE" to its closing ">", which must be tracked through quoted strings (a declaration can quote a
        // system identifier) and through the "[ ... ]" of an internal subset (which can contain ">" inside entity declarations).
        int declarationStart = text.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
        while (declarationStart >= 0)
        {
            char activeQuote = '\0';
            int internalSubsetDepth = 0;
            int declarationEnd = -1;
            for (int index = declarationStart + "<!DOCTYPE".Length; index < text.Length; index++)
            {
                char character = text[index];
                if (activeQuote != '\0')
                {
                    if (character == activeQuote)
                        activeQuote = '\0';
                    continue;
                }
                if (character is '"' or '\'')
                {
                    activeQuote = character;
                    continue;
                }
                if (character == '[')
                {
                    internalSubsetDepth++;
                    continue;
                }
                if (character == ']')
                {
                    internalSubsetDepth--;
                    continue;
                }
                if (character == '>' && internalSubsetDepth <= 0)
                {
                    declarationEnd = index;
                    break;
                }
            }
            if (declarationEnd < 0)
                break;
            text = text.Remove(declarationStart, declarationEnd - declarationStart + 1);
            declarationStart = text.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    /// <summary>
    /// Resolves the HTML named character references of an XML or XHTML document into the characters they stand for. The five entities that
    /// XML defines itself and the numeric references are left to the XML parser; an entity that is not mapped is left untouched, so that the
    /// parse then fails and the caller falls back to its existing resilience path.
    /// </summary>
    /// <param name="text">The text of the document.</param>
    /// <returns>The text of the document with its mapped HTML named character references resolved.</returns>
    private static string ResolveNamedCharacterReferences(string text)
    {
        return s_namedCharacterReferencePattern.Replace(text, match =>
        {
            string entityName = match.Groups[1].Value;
            if (entityName is "amp" or "lt" or "gt" or "quot" or "apos")
                return match.Value;
            return s_htmlNamedCharacterReferences.TryGetValue(entityName, out char character) ? character.ToString() : match.Value;
        });
    }

    /// <summary>
    /// Reads an XML entry of the archive with hardened XML settings.
    /// </summary>
    /// <param name="zipArchive">The EPUB archive.</param>
    /// <param name="entryPath">The archive path of the entry.</param>
    /// <param name="maxBytes">The maximum allowed size of the entry.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The parsed XML document.</returns>
    private static XDocument ReadXmlEntry(ZipArchive zipArchive, string entryPath, long maxBytes, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        ZipArchiveEntry? zipArchiveEntry = zipArchive.GetEntry(entryPath) ?? throw new InvalidDataException($"The archive contains no entry at '{entryPath}'.");
        byte[] data = ReadEntry(zipArchiveEntry, maxBytes, ref totalExpandedBytes, cancellationToken);

        // Every XML document of the EPUB is untrusted input, so DTD processing and external entity resolution are disabled (a DTD could declare entities that read local files, or expand into the "billion laughs" attack), and the size of the document is bounded.
        // The content is normalized first (see NormalizeDocumentText), so that the document type declaration the document may carry - and its
        // external DTD - is removed before the XML parser sees it, without loosening the hardened settings above.
        string normalizedContent = NormalizeDocumentText(data);
        XmlReaderSettings xmlReaderSettings = new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = maxBytes,
            MaxCharactersFromEntities = 0
        };
        using StringReader stringReader = new(normalizedContent);
        using XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings);
        return XDocument.Load(xmlReader, LoadOptions.None);
    }

    /// <summary>
    /// Reads the content of an archive entry, bounded by <paramref name="maxBytes"/>.
    /// </summary>
    /// <param name="entry">The archive entry to read.</param>
    /// <param name="maxBytes">The maximum allowed size of the entry.</param>
    /// <param name="totalExpandedBytes">The running total of the bytes actually read from the archive, so that the cap on the total expanded size is enforced on the streamed content rather than on the untrustworthy declared sizes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The content of the archive entry.</returns>
    private static byte[] ReadEntry(ZipArchiveEntry entry, long maxBytes, ref long totalExpandedBytes, CancellationToken cancellationToken)
    {
        // The declared size of the entry is checked up front, as a cheap first guard, but it is not trustworthy, so the bound is enforced again while the entry is streamed,
        // and every byte actually read is added to the running total of the whole EPUB, so that no sum of small entries can bypass the total-expanded cap.
        if (entry.Length > maxBytes)
            throw new InvalidDataException($"The archive entry '{entry.FullName}' is too large.");

        // The entry is then streamed into memory while the limit is enforced again on the bytes actually read, because the declared size is not trustworthy and a hostile archive could lie about it.
        using Stream stream = entry.Open();
        using MemoryStream memoryStream = new();
        byte[] buffer = new byte[64 * 1024];
        long totalBytes = 0;
        while (true)
        {
            int read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
                break;
            cancellationToken.ThrowIfCancellationRequested();
            totalBytes += read;
            if (totalBytes > maxBytes)
                throw new InvalidDataException($"The archive entry '{entry.FullName}' is too large.");
            totalExpandedBytes += read;
            if (totalExpandedBytes > MAX_TOTAL_EXPANDED_BYTES)
                throw new InvalidDataException("The expanded EPUB archive is too large.");
            memoryStream.Write(buffer, 0, read);
        }
        return memoryStream.ToArray();
    }
}
