#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
#endregion

namespace Lumina.Plugins.Epub.Fixtures.Core.Epub;

/// <summary>
/// Builds minimal, structurally valid EPUB files used as test input for the EPUB parsing tests.
/// The files are produced into a temporary directory at test runtime, because an EPUB is a binary ZIP archive.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestEpubFileFactory
{
    private const string CONTAINER_XML = """
        <?xml version="1.0" encoding="UTF-8"?>
        <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
          <rootfiles>
            <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
          </rootfiles>
        </container>
        """;

    private const string CONTENT_OPF = """
        <?xml version="1.0" encoding="UTF-8"?>
        <package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="book-id">
          <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
            <dc:title>Minimal EPUB Book</dc:title>
            <dc:creator>Test Author</dc:creator>
            <dc:language>en</dc:language>
            <meta name="cover" content="cover-image"/>
          </metadata>
          <manifest>
            <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
            <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
            <item id="chapter2" href="chapter2.xhtml" media-type="application/xhtml+xml"/>
            <item id="cover-image" href="images/cover.png" media-type="image/png" properties="cover-image"/>
          </manifest>
          <spine>
            <itemref idref="chapter1"/>
            <itemref idref="chapter2"/>
          </spine>
        </package>
        """;

    private const string NAV_XHTML = """
        <?xml version="1.0" encoding="UTF-8"?>
        <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
          <body>
            <nav epub:type="toc" id="toc">
              <ol>
                <li><a href="chapter1.xhtml">Chapter One</a>
                  <ol>
                    <li><a href="chapter1.xhtml#part">Part One</a></li>
                  </ol>
                </li>
                <li><a href="chapter2.xhtml">Chapter Two</a></li>
                <li><span>No Anchor</span></li>
              </ol>
            </nav>
          </body>
        </html>
        """;

    private const string CHAPTER1_XHTML = """
        <?xml version="1.0" encoding="UTF-8"?>
        <html xmlns="http://www.w3.org/1999/xhtml">
          <head><title>Chapter One</title></head>
          <body>
            <h1 id="part">Chapter One</h1>
            <p>First paragraph of chapter one.</p>
            <img src="images/cover.png" alt="Cover"/>
            <a href="chapter2.xhtml">Next</a>
          </body>
        </html>
        """;

    private const string CHAPTER2_XHTML = """
        <?xml version="1.0" encoding="UTF-8"?>
        <html xmlns="http://www.w3.org/1999/xhtml">
          <head><title>Chapter Two</title></head>
          <body>
            <h1>Chapter Two</h1>
            <p>Second paragraph of chapter two.</p>
          </body>
        </html>
        """;

    private const string BROKEN_SECTION_XHTML = """
        <html><body><h1>Not well formed</h1><img src="images/cover.png"></body></html>
        """;

    // A valid 1x1 PNG image, used as the cover resource of the test EPUB.
    private static readonly byte[] s_coverPng = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    /// <summary>
    /// Creates a minimal valid EPUB with two sections, a navigation document, and a cover image.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateMinimalEpub(string epubPath)
    {
        CreateEpub(epubPath, CONTENT_OPF, CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose package document declares no spine, so it has no reading sections.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithoutSpine(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Empty Spine</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine/>
            </package>
            """;
        CreateEpub(epubPath, opf, CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB with no container document at all, so no OPF file can be found.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithoutContainer(string epubPath)
    {
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "OEBPS/content.opf", CONTENT_OPF);
    }

    /// <summary>
    /// Creates an EPUB whose manifest href attempts to escape the archive.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithTraversalHref(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Traversal Href</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="../../escape.xhtml" media-type="application/xhtml+xml"/>
                <item id="chapter2" href="chapter2.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
                <itemref idref="chapter2"/>
              </spine>
            </package>
            """;
        CreateEpub(epubPath, opf, CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose second section is not well formed XML.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithBrokenSection(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Broken Section</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", BROKEN_SECTION_XHTML);
    }

    /// <summary>
    /// Creates an EPUB with the provided package and first section content.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    /// <param name="opfContent">The content of the package document.</param>
    /// <param name="chapter1Content">The content of the first section.</param>
    private static void CreateEpub(string epubPath, string opfContent, string chapter1Content)
    {
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opfContent);
        AddEntry(zipArchive, "OEBPS/nav.xhtml", NAV_XHTML);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", chapter1Content);
        AddEntry(zipArchive, "OEBPS/chapter2.xhtml", CHAPTER2_XHTML);
        AddEntry(zipArchive, "OEBPS/images/cover.png", s_coverPng);
    }

    /// <summary>
    /// Creates an EPUB 2 whose table of contents comes from an NCX document, whose cover is declared with a meta
    /// element instead of the cover-image property, and whose spine declares no EPUB 3 navigation document.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithNcxToc(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="2.0" unique-identifier="book-id">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>EPUB 2 Book</dc:title>
                <dc:creator>NCX Author</dc:creator>
                <dc:language>en</dc:language>
                <meta name="cover" content="cover-image"/>
              </metadata>
              <manifest>
                <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="chapter2" href="chapter2.xhtml" media-type="application/xhtml+xml"/>
                <item id="cover-image" href="images/cover.png" media-type="image/png"/>
              </manifest>
              <spine toc="ncx">
                <itemref idref="chapter1"/>
                <itemref idref="chapter2"/>
              </spine>
            </package>
            """;
        string ncx = """
            <?xml version="1.0" encoding="UTF-8"?>
            <ncx xmlns="http://www.daisy.org/z3986/2005/ncx/" version="2005-1">
              <navMap>
                <navPoint id="np-1">
                  <navLabel><text>NCX Chapter One</text></navLabel>
                  <content src="chapter1.xhtml"/>
                  <navPoint id="np-1-1">
                    <navLabel><text>NCX Part One</text></navLabel>
                    <content src="chapter1.xhtml#part"/>
                  </navPoint>
                </navPoint>
                <navPoint id="np-2">
                  <navLabel><text>NCX Chapter Two</text></navLabel>
                  <content src="chapter2.xhtml"/>
                </navPoint>
                <navPoint id="np-3">
                  <content src="missing.xhtml"/>
                </navPoint>
                <navPoint id="np-4">
                  <navLabel><text>   </text></navLabel>
                  <content/>
                </navPoint>
              </navMap>
            </ncx>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/toc.ncx", ncx);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
        AddEntry(zipArchive, "OEBPS/chapter2.xhtml", CHAPTER2_XHTML);
        AddEntry(zipArchive, "OEBPS/images/cover.png", s_coverPng);
    }

    /// <summary>
    /// Creates an EPUB whose manifest carries items of many media types (font, audio, video, CSS, script) so that the
    /// media-type classification of the resources is exercised.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithVariedMediaTypes(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Varied Media</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="font" href="fonts/book.woff2" media-type="font/woff2"/>
                <item id="otf" href="fonts/book.otf" media-type="application/vnd.ms-opentype"/>
                <item id="ttf" href="fonts/book.ttf" media-type="application/x-font-ttf"/>
                <item id="audio" href="audio/intro.mp3" media-type="audio/mpeg"/>
                <item id="video" href="video/clip.mp4" media-type="video/mp4"/>
                <item id="css" href="styles/book.css" media-type="text/css"/>
                <item id="script" href="scripts/main.js" media-type="application/javascript"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
        AddEntry(zipArchive, "OEBPS/fonts/book.woff2", [1, 2, 3, 4]);
        AddEntry(zipArchive, "OEBPS/fonts/book.otf", [1, 2, 3, 4]);
        AddEntry(zipArchive, "OEBPS/fonts/book.ttf", [1, 2, 3, 4]);
        AddEntry(zipArchive, "OEBPS/audio/intro.mp3", [1, 2, 3, 4]);
        AddEntry(zipArchive, "OEBPS/video/clip.mp4", [1, 2, 3, 4]);
        AddEntry(zipArchive, "OEBPS/styles/book.css", "body { color: black; }");
        AddEntry(zipArchive, "OEBPS/scripts/main.js", "var x = 1;");
    }

    /// <summary>
    /// Creates an EPUB whose container document is not well formed XML, so no OPF file can be found.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithMalformedContainer(string epubPath)
    {
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", "<container><rootfiles></container>");
    }

    /// <summary>
    /// Creates an EPUB whose navigation document contains a nav element with a non-toc type, so the table of contents is empty.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithNonTocNav(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Non Toc Nav</dc:title>
              </metadata>
              <manifest>
                <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        string nav = """
            <?xml version="1.0" encoding="UTF-8"?>
            <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
              <body>
                <nav epub:type="page-list" id="pages">
                  <ol><li><a href="chapter1.xhtml">Page One</a></li></ol>
                </nav>
              </body>
            </html>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/nav.xhtml", nav);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose package declares no title and no author, so the file name is used as the title.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithoutTitle(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:language>en</dc:language>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose package document contains broken manifest items (one without an id, one without an href),
    /// items without a properties attribute, and a spine referencing an unknown manifest Id, all of which the parser
    /// must tolerate while keeping the valid sections readable.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithBrokenManifest(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Broken Manifest</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="chapter2" href="chapter2.xhtml" media-type="application/xhtml+xml"/>
                <item href="no-id.xhtml" media-type="application/xhtml+xml"/>
                <item id="no-href" media-type="application/xhtml+xml"/>
                <item id="plain" href="plain.xhtml" media-type="application/xhtml+xml"/>
                <item id="absolute" href="/escape.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
                <itemref idref="missing-item"/>
                <itemref idref="chapter2"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
        AddEntry(zipArchive, "OEBPS/chapter2.xhtml", CHAPTER2_XHTML);
        AddEntry(zipArchive, "OEBPS/plain.xhtml", CHAPTER2_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose archive declares more entries than the parser allows, so the archive validation rejects it.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithTooManyEntries(string epubPath)
    {
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        // 20,001 entries exceeds the parser's 20,000 entry cap.
        for (int index = 0; index < 20_001; index++)
            AddEntry(zipArchive, $"OEBPS/empty-{index}.txt", "x");
    }

    /// <summary>
    /// Creates an EPUB whose entries declare a total expanded size larger than the parser allows, so the archive validation rejects it.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithTooLargeExpandedSize(string epubPath)
    {
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        // A single entry declaring more than the 256 MB expanded cap, written as repeated zeros so it stays fast.
        AddEntry(zipArchive, "OEBPS/huge.txt", new byte[300 * 1024 * 1024]);
    }

    /// <summary>
    /// Creates an EPUB whose container document carries a rootfile element without a full-path attribute, so no OPF file can be found.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithRootfileWithoutPath(string epubPath)
    {
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", """
            <?xml version="1.0" encoding="UTF-8"?>
            <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
              <rootfiles>
                <rootfile media-type="application/oebps-package+xml"/>
              </rootfiles>
            </container>
            """);
    }

    /// <summary>
    /// Creates an EPUB whose package document declares a manifest item whose file is not present in the archive, and whose
    /// spine references a section whose file is not present in the archive.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithMissingArchiveFiles(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Missing Files</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="missing-section" href="missing-section.xhtml" media-type="application/xhtml+xml"/>
                <item id="image" href="images/missing.png" media-type="image/png"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
                <itemref idref="missing-section"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose manifest declares two items with the same href, so they map to the same resource key.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithDuplicateResource(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Duplicate Resource</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="image-a" href="images/cover.png" media-type="image/png"/>
                <item id="image-b" href="images/cover.png" media-type="image/png"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
        AddEntry(zipArchive, "OEBPS/images/cover.png", s_coverPng);
    }

    /// <summary>
    /// Creates an EPUB whose navigation document is not well formed XML, so its table of contents cannot be read.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithMalformedNavigationDocument(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Malformed Nav</dc:title>
              </metadata>
              <manifest>
                <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/nav.xhtml", "<html><body><nav epub:type=\"toc\"><ol></body></html>");
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose NCX document is not well formed XML, so its table of contents cannot be read.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithMalformedNcx(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="2.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Malformed NCX</dc:title>
              </metadata>
              <manifest>
                <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine toc="ncx">
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/toc.ncx", "<ncx><navMap><navPoint></ncx>");
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose section carries every kind of reference the rewrite must handle: a valid internal image,
    /// an empty reference, a same-page fragment, a data URI, absolute web links, a mailto link, and a broken internal path.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithVariedReferences(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Varied References</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="image" href="images/cover.png" media-type="image/png"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        string section = """
            <?xml version="1.0" encoding="UTF-8"?>
            <html xmlns="http://www.w3.org/1999/xhtml">
              <head><title>Varied References</title></head>
              <body>
                <img src="images/cover.png" alt="Cover"/>
                <img src="" alt="Empty"/>
                <img src="  " alt="Blank"/>
                <a href="#part">Anchor</a>
                <img src="data:image/png;base64,AAAA" alt="Data"/>
                <a href="http://example.com">Http</a>
                <a href="https://example.com">Https</a>
                <a href="mailto:a@b.com">Mail</a>
                <a href="tel:+123">Tel</a>
                <img src="missing/image.png" alt="Broken"/>
                <a href="images/cover.png#part">With Fragment</a>
              </body>
            </html>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", section);
        AddEntry(zipArchive, "OEBPS/images/cover.png", s_coverPng);
    }

    /// <summary>
    /// Creates an EPUB whose container document points at an OPF file that is not present in the archive, so parsing fails.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithMissingOpf(string epubPath)
    {
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", """
            <?xml version="1.0" encoding="UTF-8"?>
            <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
              <rootfiles>
                <rootfile full-path="OEBPS/missing.opf" media-type="application/oebps-package+xml"/>
              </rootfiles>
            </container>
            """);
    }

    /// <summary>
    /// Creates an EPUB whose OPF package file sits at the root of the archive, so the package directory is empty.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithRootLevelOpf(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Root Level OPF</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", """
            <?xml version="1.0" encoding="UTF-8"?>
            <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
              <rootfiles>
                <rootfile full-path="content.opf" media-type="application/oebps-package+xml"/>
              </rootfiles>
            </container>
            """);
        AddEntry(zipArchive, "content.opf", opf);
        AddEntry(zipArchive, "chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose resource item has an escaping href, so it cannot be resolved to an archive path.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithEscapingResourceHref(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Escaping Resource</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="image" href="../../outside.png" media-type="image/png"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose resource file exceeds the per-resource size cap, so reading it fails.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithOversizedResource(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Oversized Resource</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="image" href="images/huge.png" media-type="image/png"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
        AddEntry(zipArchive, "OEBPS/images/huge.png", new byte[21 * 1024 * 1024]);
    }

    /// <summary>
    /// Creates an EPUB whose package document declares no metadata element, so the title falls back to the file name and the author stays null.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithoutMetadataElement(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
    }

    /// <summary>
    /// Creates an EPUB whose manifest item declares no media type.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithMissingMediaType(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Missing Media Type</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="image" href="images/cover.png"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
        AddEntry(zipArchive, "OEBPS/images/cover.png", s_coverPng);
    }

    /// <summary>
    /// Creates an EPUB whose reading section declares a DTD and uses the named character references the DTD defines, like the
    /// XHTML content documents of real books: the parser must strip the declaration and resolve the references (for example
    /// <c>&amp;nbsp;</c>) instead of failing and serving the section unrewritten.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithDoctypeAndNamedEntities(string epubPath)
    {
        string opf = """
            <?xml version="1.0" encoding="UTF-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0">
              <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Doctype Entities</dc:title>
              </metadata>
              <manifest>
                <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                <item id="image" href="images/cover.png" media-type="image/png"/>
              </manifest>
              <spine>
                <itemref idref="chapter1"/>
              </spine>
            </package>
            """;
        string section = """
            <?xml version="1.0" encoding="UTF-8"?>
            <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
            <html xmlns="http://www.w3.org/1999/xhtml">
              <head><title>Chapter One</title></head>
              <body>
                <h1>Chapter&nbsp;One</h1>
                <p>First&nbsp;paragraph&nbsp;with&nbsp;spaces.</p>
                <img src="images/cover.png" alt="Cover"/>
              </body>
            </html>
            """;
        using FileStream fileStream = File.Create(epubPath);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create);
        AddEntry(zipArchive, "mimetype", "application/epub+zip");
        AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
        AddEntry(zipArchive, "OEBPS/content.opf", opf);
        AddEntry(zipArchive, "OEBPS/chapter1.xhtml", section);
        AddEntry(zipArchive, "OEBPS/images/cover.png", s_coverPng);
    }

    /// <summary>
    /// Creates an EPUB whose individual entries each stay under the per-entry size caps, but whose combined actual expanded
    /// bytes exceed the parser's total-expanded cap while every entry declares a tiny uncompressed size. This models a hostile
    /// archive that lies about its declared sizes, so only the budget that counts the bytes actually read while the entries are
    /// streamed can reject it.
    /// </summary>
    /// <param name="epubPath">The path where the EPUB is written.</param>
    public static void CreateEpubWithUnderPerEntryCapsButOversizedExpandedSize(string epubPath)
    {
        const int RESOURCE_COUNT = 13;
        const long ACTUAL_RESOURCE_BYTES = 20L * 1024 * 1024;
        const int DECLARED_RESOURCE_BYTES = 4;

        // The resources are stored without compression, so a header patched to declare a tiny uncompressed size still streams
        // its full content back when read: the reader bounds a stored entry by its compressed size, not by its declared length.
        // Each resource alone fits under the 20 MB per-resource cap, while thirteen of them together exceed the 256 MB
        // total-expanded cap once the actual bytes are counted.
        StringBuilder opfBuilder = new();
        opfBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        opfBuilder.Append("<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"3.0\">\n");
        opfBuilder.Append("  <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n");
        opfBuilder.Append("    <dc:title>Bomb Declared Small</dc:title>\n");
        opfBuilder.Append("  </metadata>\n");
        opfBuilder.Append("  <manifest>\n");
        opfBuilder.Append("    <item id=\"chapter1\" href=\"chapter1.xhtml\" media-type=\"application/xhtml+xml\"/>\n");
        string[] resourcePaths = new string[RESOURCE_COUNT];
        for (int index = 0; index < RESOURCE_COUNT; index++)
        {
            resourcePaths[index] = $"OEBPS/resources/bomb-{index}.bin";
            opfBuilder.Append($"    <item id=\"resource-{index}\" href=\"resources/bomb-{index}.bin\" media-type=\"image/png\"/>\n");
        }
        opfBuilder.Append("  </manifest>\n");
        opfBuilder.Append("  <spine>\n");
        opfBuilder.Append("    <itemref idref=\"chapter1\"/>\n");
        opfBuilder.Append("  </spine>\n");
        opfBuilder.Append("</package>\n");

        byte[] resourceContent = new byte[ACTUAL_RESOURCE_BYTES];
        int archiveCapacity = (int)(ACTUAL_RESOURCE_BYTES * RESOURCE_COUNT) + (4 * 1024 * 1024);
        using MemoryStream memoryStream = new(capacity: archiveCapacity);
        using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zipArchive, "mimetype", "application/epub+zip");
            AddEntry(zipArchive, "META-INF/container.xml", CONTAINER_XML);
            AddEntry(zipArchive, "OEBPS/content.opf", opfBuilder.ToString());
            AddEntry(zipArchive, "OEBPS/chapter1.xhtml", CHAPTER1_XHTML);
            foreach (string resourcePath in resourcePaths)
                AddStoredEntry(zipArchive, resourcePath, resourceContent);
        }

        byte[] archiveBytes = memoryStream.GetBuffer();
        int archiveLength = (int)memoryStream.Length;
        PatchDeclaredUncompressedSizes(archiveBytes, archiveLength, resourcePaths, DECLARED_RESOURCE_BYTES);

        using FileStream fileStream = File.Create(epubPath);
        fileStream.Write(archiveBytes, 0, archiveLength);
    }

    /// <summary>
    /// Adds a text entry to the ZIP archive.
    /// </summary>
    /// <param name="zipArchive">The archive to add the entry to.</param>
    /// <param name="entryPath">The archive path of the entry.</param>
    /// <param name="content">The content of the entry.</param>
    private static void AddEntry(ZipArchive zipArchive, string entryPath, string content)
    {
        ZipArchiveEntry entry = zipArchive.CreateEntry(entryPath);
        using StreamWriter writer = new(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }

    /// <summary>
    /// Adds a binary entry to the ZIP archive.
    /// </summary>
    /// <param name="zipArchive">The archive to add the entry to.</param>
    /// <param name="entryPath">The archive path of the entry.</param>
    /// <param name="content">The content of the entry.</param>
    private static void AddEntry(ZipArchive zipArchive, string entryPath, byte[] content)
    {
        ZipArchiveEntry entry = zipArchive.CreateEntry(entryPath);
        using Stream entryStream = entry.Open();
        entryStream.Write(content, 0, content.Length);
    }

    /// <summary>
    /// Adds an entry stored without compression to the ZIP archive, so that the stored content still streams back in full when
    /// the declared uncompressed size of its headers is patched down afterwards.
    /// </summary>
    /// <param name="zipArchive">The archive to add the entry to.</param>
    /// <param name="entryPath">The archive path of the entry.</param>
    /// <param name="content">The content of the entry.</param>
    private static void AddStoredEntry(ZipArchive zipArchive, string entryPath, byte[] content)
    {
        ZipArchiveEntry entry = zipArchive.CreateEntry(entryPath, CompressionLevel.NoCompression);
        using Stream entryStream = entry.Open();
        entryStream.Write(content, 0, content.Length);
    }

    /// <summary>
    /// Overwrites the declared uncompressed size of the given stored entries in both the local file headers and the central
    /// directory, so that an archive which expands into far more data than it declares can be simulated; the stored content is
    /// left untouched, so it still streams back in full.
    /// </summary>
    /// <param name="archiveBytes">The raw bytes of the ZIP archive.</param>
    /// <param name="archiveLength">The length of the archive, which may be smaller than the capacity of <paramref name="archiveBytes"/>.</param>
    /// <param name="entryPaths">The archive paths of the entries whose declared sizes are overwritten.</param>
    /// <param name="declaredSize">The declared uncompressed size to write into the headers.</param>
    private static void PatchDeclaredUncompressedSizes(byte[] archiveBytes, int archiveLength, IReadOnlyList<string> entryPaths, int declaredSize)
    {
        HashSet<string> pathsToPatch = [.. entryPaths];
        byte[] declaredSizeBytes = BitConverter.GetBytes((uint)declaredSize);
        // The reader bounds a stored entry by its compressed size while streaming it, but reports the declared uncompressed size
        // as its Length, so both the local file header and the central directory record carry the size that has to be patched.
        for (int offset = 0; offset <= archiveLength - 4; offset++)
        {
            bool isLocalFileHeader = archiveBytes[offset] == 0x50 && archiveBytes[offset + 1] == 0x4B && archiveBytes[offset + 2] == 0x03 && archiveBytes[offset + 3] == 0x04;
            if (isLocalFileHeader)
            {
                int nameOffset = offset + 30;
                int nameLength = archiveBytes[offset + 26] | (archiveBytes[offset + 27] << 8);
                if (nameOffset + nameLength <= archiveLength && pathsToPatch.Contains(Encoding.UTF8.GetString(archiveBytes, nameOffset, nameLength)))
                    Array.Copy(declaredSizeBytes, 0, archiveBytes, offset + 22, declaredSizeBytes.Length);
            }

            bool isCentralDirectoryHeader = archiveBytes[offset] == 0x50 && archiveBytes[offset + 1] == 0x4B && archiveBytes[offset + 2] == 0x01 && archiveBytes[offset + 3] == 0x02;
            if (isCentralDirectoryHeader)
            {
                int nameOffset = offset + 46;
                int nameLength = archiveBytes[offset + 28] | (archiveBytes[offset + 29] << 8);
                if (nameOffset + nameLength <= archiveLength && pathsToPatch.Contains(Encoding.UTF8.GetString(archiveBytes, nameOffset, nameLength)))
                    Array.Copy(declaredSizeBytes, 0, archiveBytes, offset + 24, declaredSizeBytes.Length);
            }
        }
    }
}
