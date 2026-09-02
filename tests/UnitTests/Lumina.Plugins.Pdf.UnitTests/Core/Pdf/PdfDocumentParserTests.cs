#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Plugins.Pdf.Core.Pdf;
using Lumina.Plugins.Pdf.Fixtures.Core.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
#endregion

namespace Lumina.Plugins.Pdf.UnitTests.Core.Pdf;

/// <summary>
/// Contains unit tests for the <see cref="PdfDocumentParser"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PdfDocumentParserTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-pdf-tests-{Guid.NewGuid():N}");
    private readonly string _pdfPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfDocumentParserTests"/> class.
    /// </summary>
    public PdfDocumentParserTests()
    {
        Directory.CreateDirectory(_temporaryDirectory);
        _pdfPath = Path.Combine(_temporaryDirectory, "minimal.pdf");
        TestPdfFileFactory.CreateMinimalPdf(_pdfPath);
    }

    [Fact]
    public void Parse_WhenCalledOnPdfWithTextLayer_ShouldReturnDocumentWithTextSections()
    {
        // Arrange
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = PdfDocumentParser.Parse(_pdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.Equal("minimal", result.Title);
        Assert.True(result.HasTextContent);
        ReadingSpineItemDto spineItem = Assert.Single(result.Spine);
        Assert.Equal("page:1", spineItem.LocationRef);
        string sectionContent = File.ReadAllText(Path.Combine(workingDirectory, spineItem.RelativeSectionFilePath));
        Assert.Contains("<p>Hello PDF Page 1</p>", sectionContent, StringComparison.Ordinal);
        Assert.Empty(result.Resources);
    }

    [Fact]
    public void Parse_WhenCalledWithRenderAsImages_ShouldReturnDocumentWithImageSectionsAndNoTextContent()
    {
        // Arrange
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = PdfDocumentParser.Parse(_pdfPath, workingDirectory, shouldRenderPdfAsImages: true, CancellationToken.None);

        // Assert
        Assert.Equal("minimal", result.Title);
        Assert.False(result.HasTextContent);
        ReadingSpineItemDto spineItem = Assert.Single(result.Spine);
        Assert.Equal("page:1", spineItem.LocationRef);
        KeyValuePair<string, ReadingResourceInfoDto> resource = Assert.Single(result.Resources);
        Assert.Equal("page:1", resource.Key);
        Assert.Equal("image/png", resource.Value.MimeType);
        string sectionContent = File.ReadAllText(Path.Combine(workingDirectory, spineItem.RelativeSectionFilePath));
        Assert.Contains("data-lumina-resource=\"page:1\"", sectionContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenPdfFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
        // Arrange
        string missingPdfPath = Path.Combine(_temporaryDirectory, "missing.pdf");
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => PdfDocumentParser.Parse(missingPdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.Throws<FileNotFoundException>(act);
    }

    [SupportedOSPlatform("windows")]
    [Fact]
    public void RenderResource_WhenResourceKeyDoesNotIdentifyAPage_ShouldThrowInvalidDataException()
    {
        // Act
        Action act = () => PdfDocumentParser.RenderResource(_pdfPath, "not-a-page", CancellationToken.None);

        // Assert
        Assert.Throws<InvalidDataException>(act);
    }

    [SupportedOSPlatform("windows")]
    [Fact]
    public void RenderResource_WhenResourceKeyNamesAPageBeyondThePageCap_ShouldThrowInvalidDataException()
    {
        // Act
        // The minimal PDF has a single page, but the key is rejected by the page cap before the document is even opened.
        Action act = () => PdfDocumentParser.RenderResource(_pdfPath, "page:10001", CancellationToken.None);

        // Assert
        Assert.Throws<InvalidDataException>(act);
    }

    [SupportedOSPlatform("windows")]
    [Fact]
    public void RenderResource_WhenResourceKeyNamesAPageBeyondTheDocument_ShouldThrowInvalidDataException()
    {
        // Act
        // The minimal PDF has a single page, so page 2 is rejected against the page count of the document without rendering it.
        Action act = () => PdfDocumentParser.RenderResource(_pdfPath, "page:2", CancellationToken.None);

        // Assert
        Assert.Throws<InvalidDataException>(act);
    }

    [Fact]
    public void Parse_WhenPdfHasBookmarks_ShouldBuildTableOfContentsFromTheOutline()
    {
        // Arrange
        string outlinedPdfPath = Path.Combine(_temporaryDirectory, "outlined.pdf");
        TestPdfFileFactory.CreatePdfWithOutline(outlinedPdfPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = PdfDocumentParser.Parse(outlinedPdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        // PdfPig does not surface a bookmark without a title, so only the three titled top-level entries appear.
        Assert.Equal(3, result.TableOfContents.Count);
        ReadingTocEntryDto chapterOne = result.TableOfContents[0];
        Assert.Equal("Chapter One", chapterOne.Label);
        Assert.Equal("page:1", chapterOne.LocationRef);
        ReadingTocEntryDto partOne = Assert.Single(chapterOne.Children);
        Assert.Equal("Part One", partOne.Label);
        Assert.Equal("page:1", partOne.LocationRef);
        Assert.Equal("Chapter Two", result.TableOfContents[1].Label);
        Assert.Equal("page:2", result.TableOfContents[1].LocationRef);
        // A bookmark pointing at a URI instead of a page yields an entry that is not navigable.
        Assert.Equal("External Link", result.TableOfContents[2].Label);
        Assert.Equal(string.Empty, result.TableOfContents[2].LocationRef);
    }

    [Fact]
    public void Parse_WhenPdfIsTooLarge_ShouldThrowInvalidDataException()
    {
        // Arrange
        string oversizedPdfPath = Path.Combine(_temporaryDirectory, "oversized.pdf");
        // A sparse file larger than the 500 MB cap, without allocating disk space.
        using (FileStream fileStream = new(oversizedPdfPath, FileMode.Create, FileAccess.Write))
            fileStream.SetLength(501L * 1024 * 1024);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => PdfDocumentParser.Parse(oversizedPdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.Throws<InvalidDataException>(act);
    }

    [Fact]
    public void Parse_WhenPdfHasABlankTextLine_ShouldSkipTheBlankLineAndKeepTheOthers()
    {
        // Arrange
        string blankLinePdfPath = Path.Combine(_temporaryDirectory, "blankline.pdf");
        TestPdfFileFactory.CreatePdfWithBlankLine(blankLinePdfPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = PdfDocumentParser.Parse(blankLinePdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        string sectionContent = File.ReadAllText(Path.Combine(workingDirectory, result.Spine[0].RelativeSectionFilePath));
        Assert.Contains("<p>First line</p>", sectionContent, StringComparison.Ordinal);
        Assert.Contains("<p>Third line</p>", sectionContent, StringComparison.Ordinal);
        Assert.DoesNotContain("<p></p>", sectionContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenPdfOutlineIsMalformed_ShouldSkipTheTableOfContentsAndKeepThePages()
    {
        // Arrange
        string malformedOutlinePdfPath = Path.Combine(_temporaryDirectory, "malformed-outline.pdf");
        TestPdfFileFactory.CreatePdfWithMalformedOutline(malformedOutlinePdfPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = PdfDocumentParser.Parse(malformedOutlinePdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        // The malformed bookmarks are best effort: the table of contents is skipped, but the pages are still readable.
        Assert.Empty(result.TableOfContents);
        Assert.Single(result.Spine);
    }

    [Fact]
    public void Parse_WhenPdfExceedsThePageCap_ShouldStopAtTheCap()
    {
        // Arrange
        string manyPagesPdfPath = Path.Combine(_temporaryDirectory, "many-pages.pdf");
        TestPdfFileFactory.CreatePdfWithMorePagesThanAllowed(manyPagesPdfPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = PdfDocumentParser.Parse(manyPagesPdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        // The parser stops enumerating after the page cap instead of reading every page.
        Assert.Equal(10_000, result.Spine.Count);
    }

    [Fact]
    public void ParseAsImages_WhenPdfExceedsThePageCap_ShouldStopAtTheCap()
    {
        // Arrange
        string manyPagesPdfPath = Path.Combine(_temporaryDirectory, "many-pages-images.pdf");
        TestPdfFileFactory.CreatePdfWithMorePagesThanAllowed(manyPagesPdfPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = PdfDocumentParser.Parse(manyPagesPdfPath, workingDirectory, shouldRenderPdfAsImages: true, CancellationToken.None);

        // Assert
        Assert.Equal(10_000, result.Spine.Count);
        Assert.Equal(10_000, result.Resources.Count);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
    }
}

