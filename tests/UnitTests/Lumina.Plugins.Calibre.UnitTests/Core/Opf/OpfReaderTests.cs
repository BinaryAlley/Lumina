#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using Lumina.Plugins.Calibre.Core.Opf;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Plugins.Calibre.UnitTests.Core.Opf;

/// <summary>
/// Contains unit tests for the <see cref="OpfReader"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpfReaderTests
{
    [Fact]
    public void Read_WhenOpfFileContainsAllSupportedFields_ShouldParseThemAll()
    {
        // Arrange
        string opfFilePath = CreateOpfFile("""<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf" version="2.0" unique-identifier="uuid_id"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:opf="http://www.idpf.org/2007/opf"><dc:title>Test Book Title</dc:title><dc:creator opf:role="aut">Test Author</dc:creator><dc:creator opf:role="oth">Second Creator</dc:creator><dc:contributor opf:role="bkp">calibre (3.48.0) [https://calibre-ebook.com]</dc:contributor><dc:contributor opf:role="trl">Test Translator</dc:contributor><dc:description>&lt;p&gt;A test description with &lt;b&gt;markup&lt;/b&gt;.&lt;/p&gt;</dc:description><dc:publisher>Test Publisher</dc:publisher><dc:date>2015-06-15</dc:date><dc:language>en</dc:language><dc:identifier opf:scheme="ISBN">978-0-306-40615-7</dc:identifier><dc:identifier opf:scheme="goodreads">123456</dc:identifier><dc:identifier opf:scheme="ASIN">B00TEST1</dc:identifier><dc:identifier opf:scheme="LCCN">test-lccn</dc:identifier><dc:identifier opf:scheme="oclc">ocm12345678</dc:identifier><dc:identifier opf:scheme="openlibrary">OL100M</dc:identifier><dc:identifier opf:scheme="google">google-id</dc:identifier><dc:subject>Science fiction</dc:subject><dc:subject>Space opera</dc:subject><meta name="calibre:series" content="The Series"/><meta name="calibre:series_index" content="2.5"/><meta name="calibre:rating" content="8"/></metadata><guide><reference type="cover" href="cover.jpg"/><reference type="toc" href="toc.xhtml"/></guide></package>""");

        try
        {
            // Act
            OpfDocumentDto result = OpfReader.Read(opfFilePath);

            // Assert
            Assert.Equal("Test Book Title", result.Title);
            Assert.Equal("<p>A test description with <b>markup</b>.</p>", result.Description);
            Assert.Equal("Test Publisher", result.Publisher);
            Assert.Equal(2015, result.PublishDate!.Value.Year);
            Assert.Equal(6, result.PublishDate.Value.Month);
            Assert.Equal(15, result.PublishDate.Value.Day);
            Assert.Equal("en", result.LanguageCode);
            Assert.Equal("The Series", result.Series);
            Assert.Equal(2.5, result.SeriesIndex);
            Assert.Equal(8, result.Rating);
            Assert.Equal("cover.jpg", result.CoverHref);
            Assert.Equal(2, result.Creators.Count);
            Assert.Equal("Test Author", result.Creators[0].Name);
            Assert.Equal("aut", result.Creators[0].Role);
            Assert.Equal(2, result.Contributors.Count);
            Assert.Equal("calibre (3.48.0) [https://calibre-ebook.com]", result.Contributors[0].Name);
            Assert.Equal("bkp", result.Contributors[0].Role);
            Assert.Equal(7, result.Identifiers.Count);
            Assert.Contains(result.Identifiers, identifier => identifier.Scheme == "ISBN" && identifier.Value == "978-0-306-40615-7");
            Assert.Contains(result.Identifiers, identifier => identifier.Scheme == "goodreads" && identifier.Value == "123456");
            Assert.Contains(result.Identifiers, identifier => identifier.Scheme == "ASIN" && identifier.Value == "B00TEST1");
            Assert.Equal(2, result.Subjects.Count);
            Assert.Equal("Science fiction", result.Subjects[0]);
            Assert.Equal("Space opera", result.Subjects[1]);
        }
        finally
        {
            File.Delete(opfFilePath);
        }
    }

    [Fact]
    public void Read_WhenOpfFileDoesNotExist_ShouldReturnEmptyDocument()
    {
        // Act
        OpfDocumentDto result = OpfReader.Read(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.opf"));

        // Assert
        Assert.Null(result.Title);
        Assert.Empty(result.Identifiers);
        Assert.Empty(result.Creators);
        Assert.Empty(result.Subjects);
    }

    [Fact]
    public void Read_WhenMetadataContainsWhitespaceOnlyValues_ShouldSkipThem()
    {
        // Arrange
        string opfFilePath = CreateOpfFile("""<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>   </dc:title><dc:creator opf:role="aut">   </dc:creator><dc:subject></dc:subject></metadata></package>""");

        try
        {
            // Act
            OpfDocumentDto result = OpfReader.Read(opfFilePath);

            // Assert
            Assert.Null(result.Title);
            Assert.Empty(result.Creators);
            Assert.Empty(result.Subjects);
        }
        finally
        {
            File.Delete(opfFilePath);
        }
    }

    [Fact]
    public void Read_WhenMetadataElementIsMissing_ShouldReturnEmptyDocument()
    {
        // Arrange
        string opfFilePath = CreateOpfFile("""<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf"></package>""");

        try
        {
            // Act
            OpfDocumentDto result = OpfReader.Read(opfFilePath);

            // Assert
            Assert.Null(result.Title);
            Assert.Empty(result.Identifiers);
        }
        finally
        {
            File.Delete(opfFilePath);
        }
    }

    [Fact]
    public void Read_WhenOpfFileDeclaresADoctypeWithEntities_ShouldNotExpandThem()
    {
        // Arrange
        // the DTD is prohibited, so the document must not be parsed and the entity must not be expanded into the title
        string opfFilePath = CreateOpfFile("""<?xml version="1.0" encoding="utf-8"?><!DOCTYPE package [<!ENTITY xxe SYSTEM "file:///etc/passwd">]><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>&xxe;</dc:title></metadata></package>""");

        try
        {
            // Act
            OpfDocumentDto result = OpfReader.Read(opfFilePath);

            // Assert
            Assert.Null(result.Title);
        }
        finally
        {
            File.Delete(opfFilePath);
        }
    }

    [Fact]
    public void Read_WhenOpfFileContainsBillionLaughsStyleEntityExpansion_ShouldNotExpandThem()
    {
        // Arrange
        string opfFilePath = CreateOpfFile("""<?xml version="1.0" encoding="utf-8"?><!DOCTYPE lolz [<!ENTITY lol "lol"><!ENTITY lol2 "&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;"><!ENTITY lol3 "&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;">]><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>&lol3;</dc:title></metadata></package>""");

        try
        {
            // Act
            OpfDocumentDto result = OpfReader.Read(opfFilePath);

            // Assert
            Assert.Null(result.Title);
        }
        finally
        {
            File.Delete(opfFilePath);
        }
    }

    [Fact]
    public void Read_WhenOpfFileIsOversized_ShouldReturnEmptyDocument()
    {
        // Arrange
        string opfFilePath = Path.Combine(Path.GetTempPath(), $"oversized-{Guid.NewGuid():N}.opf");
        string oversizedContent = $"""<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>{new string('a', (5 * 1024 * 1024) + 100)}</dc:title></metadata></package>""";
        File.WriteAllText(opfFilePath, oversizedContent);

        try
        {
            // Act
            OpfDocumentDto result = OpfReader.Read(opfFilePath);

            // Assert
            Assert.Null(result.Title);
            Assert.Empty(result.Identifiers);
        }
        finally
        {
            File.Delete(opfFilePath);
        }
    }

    [Fact]
    public void Read_WhenOpfFileIsMalformedXml_ShouldReturnEmptyDocument()
    {
        // Arrange
        string opfFilePath = CreateOpfFile("""<package xmlns="http://www.idpf.org/2007/opf"><metadata><dc:title>Unclosed""");

        try
        {
            // Act
            OpfDocumentDto result = OpfReader.Read(opfFilePath);

            // Assert
            Assert.Null(result.Title);
            Assert.Empty(result.Identifiers);
        }
        finally
        {
            File.Delete(opfFilePath);
        }
    }

    /// <summary>
    /// Creates a temporary OPF file with the given content and returns its path.
    /// </summary>
    /// <param name="content">The content of the OPF file to create.</param>
    /// <returns>The file system path of the created OPF file.</returns>
    private static string CreateOpfFile(string content)
    {
        string opfFilePath = Path.Combine(Path.GetTempPath(), $"opf-{Guid.NewGuid():N}.opf");
        File.WriteAllText(opfFilePath, content);
        return opfFilePath;
    }
}
