#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Plugins.Epub.Core.Epub;
using Lumina.Plugins.Epub.Fixtures.Core.Epub;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
#endregion

namespace Lumina.Plugins.Epub.UnitTests.Core.Epub;

/// <summary>
/// Contains unit tests for the <see cref="EpubDocumentParser"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpubDocumentParserTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-epub-tests-{Guid.NewGuid():N}");
    private readonly string _epubPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="EpubDocumentParserTests"/> class.
    /// </summary>
    public EpubDocumentParserTests()
    {
        Directory.CreateDirectory(_temporaryDirectory);
        _epubPath = Path.Combine(_temporaryDirectory, "minimal.epub");
    }

    [Fact]
    public void Parse_WhenCalledOnValidEpub_ShouldReturnDocumentWithTitleAuthorAndSpine()
    {
        // Arrange
        TestEpubFileFactory.CreateMinimalEpub(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Equal("Minimal EPUB Book", result.Title);
        Assert.Equal("Test Author", result.Author);
        Assert.Equal(2, result.Spine.Count);
        Assert.Equal("chapter1", result.Spine[0].LocationRef);
        Assert.Equal("chapter2", result.Spine[1].LocationRef);
        Assert.True(result.HasTextContent);
    }

    [Fact]
    public void Parse_WhenCalledOnValidEpub_ShouldExtractResourcesAndRewriteReferences()
    {
        // Arrange
        TestEpubFileFactory.CreateMinimalEpub(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        string expectedKey = CreateResourceKey("images/cover.png");
        KeyValuePair<string, ReadingResourceInfoDto> resource = Assert.Single(result.Resources);
        Assert.Equal(expectedKey, resource.Key);
        Assert.Equal("image/png", resource.Value.MimeType);
        Assert.True(File.Exists(Path.Combine(workingDirectory, resource.Value.RelativeFilePath)));
        string sectionContent = File.ReadAllText(Path.Combine(workingDirectory, result.Spine[0].RelativeSectionFilePath));
        Assert.Contains($"data-lumina-resource=\"{expectedKey}\"", sectionContent, StringComparison.Ordinal);
        Assert.DoesNotContain("src=\"images/cover.png\"", sectionContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenSectionDeclaresADoctypeAndNamedEntities_ShouldRewriteItsReferencesAndResolveTheEntities()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithDoctypeAndNamedEntities(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        ReadingSpineItemDto spineItem = Assert.Single(result.Spine);
        string sectionContent = File.ReadAllText(Path.Combine(workingDirectory, spineItem.RelativeSectionFilePath));
        string expectedKey = CreateResourceKey("images/cover.png");
        // The DTD declaration was tolerated and the named character references resolved, so the section was rewritten instead of
        // being served "as-is": the internal image reference became the resource marker and the non-breaking spaces were decoded.
        Assert.Contains($"data-lumina-resource=\"{expectedKey}\"", sectionContent, StringComparison.Ordinal);
        Assert.DoesNotContain("src=\"images/cover.png\"", sectionContent, StringComparison.Ordinal);
        Assert.DoesNotContain("<!DOCTYPE", sectionContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("&nbsp;", sectionContent, StringComparison.Ordinal);
        Assert.Contains("Chapter\u00A0One", sectionContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenCalledOnValidEpub_ShouldBuildTableOfContentsFromTheNavigationDocument()
    {
        // Arrange
        TestEpubFileFactory.CreateMinimalEpub(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // The parser collects every list item of the navigation document, including the nested ones, so the nested
        // "Part One" entry is present both at the top level and as a child of its parent entry; the anchor-less item
        // yields a non-navigable entry.
        Assert.Equal(4, result.TableOfContents.Count);
        ReadingTocEntryDto firstEntry = result.TableOfContents[0];
        Assert.Equal("Chapter One", firstEntry.Label);
        Assert.Equal("chapter1", firstEntry.LocationRef);
        ReadingTocEntryDto childEntry = Assert.Single(firstEntry.Children);
        Assert.Equal("Part One", childEntry.Label);
        Assert.Equal("chapter1", childEntry.LocationRef);
        Assert.Equal("Part One", result.TableOfContents[1].Label);
        Assert.Equal("chapter1", result.TableOfContents[1].LocationRef);
        Assert.Equal("Chapter Two", result.TableOfContents[2].Label);
        Assert.Equal("chapter2", result.TableOfContents[2].LocationRef);
        Assert.Equal(string.Empty, result.TableOfContents[3].Label);
        Assert.Equal(string.Empty, result.TableOfContents[3].LocationRef);
        Assert.Equal("Chapter One", result.Spine[0].Title);
        Assert.Equal("Chapter Two", result.Spine[1].Title);
    }

    [Fact]
    public void Parse_WhenCalledOnValidEpub_ShouldExposeTheCoverResourceKey()
    {
        // Arrange
        TestEpubFileFactory.CreateMinimalEpub(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Equal(CreateResourceKey("images/cover.png"), result.CoverResourceKey);
    }

    [Fact]
    public void Parse_WhenEpubHasNoContainerDocument_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithoutContainer(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("OPF", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenEpubHasNoSpine_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithoutSpine(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("reading sections", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenEpubHasTraversalHref_ShouldSkipTheBrokenItemAndKeepTheRest()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithTraversalHref(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // The broken manifest item is skipped, while the resolvable section is still parsed.
        Assert.Equal("Traversal Href", result.Title);
        ReadingSpineItemDto spineItem = Assert.Single(result.Spine);
        Assert.Equal("chapter2", spineItem.LocationRef);
    }

    [Fact]
    public void Parse_WhenSectionIsNotWellFormedXml_ShouldServeTheSectionAsIs()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithBrokenSection(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        ReadingSpineItemDto spineItem = Assert.Single(result.Spine);
        string sectionContent = File.ReadAllText(Path.Combine(workingDirectory, spineItem.RelativeSectionFilePath));
        Assert.Contains("Not well formed", sectionContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenEpub2HasNcxToc_ShouldBuildTableOfContentsFromTheNcxDocument()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithNcxToc(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Equal("EPUB 2 Book", result.Title);
        // The NCX entries: Chapter One (with its nested Part One), Chapter Two, the anchor-less entry whose target is
        // missing, and the entry with a blank label and no content source.
        Assert.Equal(4, result.TableOfContents.Count);
        ReadingTocEntryDto firstEntry = result.TableOfContents[0];
        Assert.Equal("NCX Chapter One", firstEntry.Label);
        Assert.Equal("chapter1", firstEntry.LocationRef);
        ReadingTocEntryDto childEntry = Assert.Single(firstEntry.Children);
        Assert.Equal("NCX Part One", childEntry.Label);
        Assert.Equal("chapter1", childEntry.LocationRef);
        Assert.Equal("NCX Chapter Two", result.TableOfContents[1].Label);
        Assert.Equal("chapter2", result.TableOfContents[1].LocationRef);
        // A navigation point without a label and pointing at an unknown file yields a non-navigable entry.
        Assert.Equal(string.Empty, result.TableOfContents[2].Label);
        Assert.Equal(string.Empty, result.TableOfContents[2].LocationRef);
        // A navigation point with a blank label and no content source also yields a non-navigable entry.
        Assert.Equal(string.Empty, result.TableOfContents[3].Label);
        Assert.Equal(string.Empty, result.TableOfContents[3].LocationRef);
        Assert.Equal("NCX Chapter One", result.Spine[0].Title);
        Assert.Equal("NCX Chapter Two", result.Spine[1].Title);
        // The cover is declared with an EPUB 2 meta element, and is still exposed.
        Assert.Equal(CreateResourceKey("images/cover.png"), result.CoverResourceKey);
    }

    [Fact]
    public void Parse_WhenEpub2HasNcxTocAndNoNavigationDocument_ShouldUseTheNcxDocument()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithNcxToc(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // No EPUB 3 navigation document is present, so every named entry comes from the NCX document.
        Assert.Equal("NCX Chapter One", result.TableOfContents[0].Label);
        Assert.Equal("NCX Chapter Two", result.TableOfContents[1].Label);
    }

    [Fact]
    public void Parse_WhenManifestCarriesVariedMediaTypes_ShouldExtractOnlyTheUsableResources()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithVariedMediaTypes(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // The font, audio, video, and CSS items are extracted as resources; the script is not a resource the reader serves.
        Assert.Equal(6, result.Resources.Count);
        Assert.Contains(result.Resources, resource => resource.Value.MimeType == "font/woff2");
        Assert.Contains(result.Resources, resource => resource.Value.MimeType == "application/vnd.ms-opentype");
        Assert.Contains(result.Resources, resource => resource.Value.MimeType == "application/x-font-ttf");
        Assert.Contains(result.Resources, resource => resource.Value.MimeType == "audio/mpeg");
        Assert.Contains(result.Resources, resource => resource.Value.MimeType == "video/mp4");
        Assert.DoesNotContain(result.Resources, resource => resource.Value.MimeType == "application/javascript");
        Assert.Contains(result.Resources, resource => resource.Value.MimeType == "text/css");
        // The section is the only spine item, so the document still reads.
        Assert.Single(result.Spine);
    }

    [Fact]
    public void Parse_WhenContainerDocumentIsMalformed_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithMalformedContainer(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("OPF", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenNavigationDocumentHasNoTocNav_ShouldReturnEmptyTableOfContents()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithNonTocNav(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Empty(result.TableOfContents);
        Assert.Single(result.Spine);
    }

    [Fact]
    public void Parse_WhenEpubHasNoTitle_ShouldUseTheFileNameAsTheTitle()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithoutTitle(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Equal("minimal", result.Title);
        Assert.Null(result.Author);
    }

    [Fact]
    public void Parse_WhenManifestContainsBrokenItems_ShouldSkipThemAndKeepTheValidSections()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithBrokenManifest(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // The two valid spine items are kept, and the reference to the unknown Id is dropped.
        Assert.Equal(2, result.Spine.Count);
        Assert.Equal("chapter1", result.Spine[0].LocationRef);
        Assert.Equal("chapter2", result.Spine[1].LocationRef);
        // The plain item (no properties) is not extracted as a resource because it is not a spine section and not a usable media type.
        Assert.Empty(result.Resources);
    }

    [Fact]
    public void Parse_WhenArchiveHasTooManyEntries_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithTooManyEntries(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("too many entries", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_WhenArchiveExpandsBeyondTheLimit_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithTooLargeExpandedSize(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("too large", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_WhenRootfileHasNoFullPath_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithRootfileWithoutPath(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("OPF", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenArchiveFilesAreMissing_ShouldSkipTheBrokenItemsAndKeepTheReadableSection()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithMissingArchiveFiles(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // The section whose file is missing from the archive is skipped, while the readable one is kept.
        Assert.Single(result.Spine);
        Assert.Equal("chapter1", result.Spine[0].LocationRef);
        // The resource whose file is missing from the archive is not extracted.
        Assert.Empty(result.Resources);
    }

    [Fact]
    public void Parse_WhenTwoItemsShareAHref_ShouldExtractTheResourceOnce()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithDuplicateResource(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        string expectedKey = CreateResourceKey("images/cover.png");
        KeyValuePair<string, ReadingResourceInfoDto> resource = Assert.Single(result.Resources);
        Assert.Equal(expectedKey, resource.Key);
    }

    [Fact]
    public void Parse_WhenNavigationDocumentIsMalformed_ShouldReturnEmptyTableOfContents()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithMalformedNavigationDocument(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Empty(result.TableOfContents);
        Assert.Single(result.Spine);
    }

    [Fact]
    public void Parse_WhenNcxDocumentIsMalformed_ShouldReturnEmptyTableOfContents()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithMalformedNcx(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Empty(result.TableOfContents);
        Assert.Single(result.Spine);
    }

    [Fact]
    public void Parse_WhenSectionCarriesVariedReferences_ShouldRewriteOnlyTheResolvableOnes()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithVariedReferences(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        string sectionContent = File.ReadAllText(Path.Combine(workingDirectory, result.Spine[0].RelativeSectionFilePath));
        string expectedKey = CreateResourceKey("images/cover.png");
        // The resolvable internal image is rewritten to the resource marker; the fragment variant resolves to the same image.
        Assert.Contains($"data-lumina-resource=\"{expectedKey}\"", sectionContent, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(sectionContent, "data-lumina-resource"));
        // The same-page anchor, data URI, absolute links, mailto, and broken path keep their original value.
        Assert.Contains("href=\"#part\"", sectionContent, StringComparison.Ordinal);
        Assert.Contains("data:image/png;base64,AAAA", sectionContent, StringComparison.Ordinal);
        Assert.Contains("href=\"http://example.com\"", sectionContent, StringComparison.Ordinal);
        Assert.Contains("href=\"https://example.com\"", sectionContent, StringComparison.Ordinal);
        Assert.Contains("href=\"mailto:a@b.com\"", sectionContent, StringComparison.Ordinal);
        Assert.Contains("href=\"tel:+123\"", sectionContent, StringComparison.Ordinal);
        Assert.Contains("src=\"missing/image.png\"", sectionContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_WhenEpubHasNoMetadataElement_ShouldUseTheFileNameAsTheTitleAndLeaveAuthorNull()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithoutMetadataElement(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Equal("minimal", result.Title);
        Assert.Null(result.Author);
        Assert.Single(result.Spine);
    }

    [Fact]
    public void Parse_WhenOpfFileIsMissing_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithMissingOpf(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Throws<InvalidDataException>(act);
    }

    [Fact]
    public void Parse_WhenOpfSitsAtTheArchiveRoot_ShouldResolveSectionsRelativeToIt()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithRootLevelOpf(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Equal("Root Level OPF", result.Title);
        ReadingSpineItemDto spineItem = Assert.Single(result.Spine);
        Assert.Equal("chapter1", spineItem.LocationRef);
    }

    [Fact]
    public void Parse_WhenResourceHrefEscapesTheArchive_ShouldSkipTheResource()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithEscapingResourceHref(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        Assert.Equal("Escaping Resource", result.Title);
        Assert.Empty(result.Resources);
        Assert.Single(result.Spine);
    }

    [Fact]
    public void Parse_WhenResourceExceedsTheSizeCap_ShouldThrowInvalidDataException()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithOversizedResource(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("too large", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_WhenEntriesFitPerEntryCapsButActualExpandedBytesExceedTheTotalCap_ShouldThrowInvalidDataException()
    {
        // Arrange
        // The entries each declare a tiny uncompressed size and each actual one is under its per-entry cap, so only the budget
        // that counts the bytes actually read while the entries are streamed can reject the archive.
        TestEpubFileFactory.CreateEpubWithUnderPerEntryCapsButOversizedExpandedSize(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // The declared sizes stay well under the 256 MB cap, so the declared-size quick guard in ValidateArchive cannot be
        // what rejects the archive; the rejection can only come from counting the bytes actually read.
        long declaredTotalBytes;
        using (FileStream fileStream = File.OpenRead(_epubPath))
        using (ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Read))
            declaredTotalBytes = zipArchive.Entries.Sum(entry => entry.Length);
        Assert.True(declaredTotalBytes <= 256 * 1024 * 1024);

        // Act
        Action act = () => EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // The total-expanded message is specific to the archive-wide cap, unlike the per-entry "too large" message.
        InvalidDataException exception = Assert.Throws<InvalidDataException>(act);
        Assert.Contains("expanded EPUB archive", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_WhenManifestItemHasNoMediaType_ShouldSkipIt()
    {
        // Arrange
        TestEpubFileFactory.CreateEpubWithMissingMediaType(_epubPath);
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = EpubDocumentParser.Parse(_epubPath, workingDirectory, CancellationToken.None);

        // Assert
        // A manifest item without a media type cannot be classified as an extractable resource, so it is skipped.
        Assert.Empty(result.Resources);
        Assert.Single(result.Spine);
    }

    /// <summary>
    /// Counts the occurrences of <paramref name="needle"/> in <paramref name="haystack"/>.
    /// </summary>
    /// <param name="haystack">The text to search.</param>
    /// <param name="needle">The substring to count.</param>
    /// <returns>The number of occurrences of the substring.</returns>
    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0;
        int index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }

    /// <summary>
    /// Computes the resource key the parser derives from a manifest href.
    /// </summary>
    /// <param name="href">The manifest href.</param>
    /// <returns>The resource key of the href.</returns>
    private static string CreateResourceKey(string href)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(href)))[..32].ToLowerInvariant();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
    }
}

