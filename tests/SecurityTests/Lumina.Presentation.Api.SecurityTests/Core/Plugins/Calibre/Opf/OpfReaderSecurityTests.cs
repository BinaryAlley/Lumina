#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Plugins.Calibre.Core;
using Lumina.Plugins.Calibre.Core.Opf;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Plugins.Calibre.Opf;

/// <summary>
/// Contains security tests for the <see cref="OpfReader"/> class, exercised through the public metadata provider surface.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpfReaderSecurityTests
{
    private const int MAX_OPF_FILE_SIZE_BYTES = 5 * 1024 * 1024;
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();

    [Fact]
    public async Task GetMetadataAsync_WhenOpfContainsADoctypeWithAnExternalEntity_ShouldNotExpandTheEntity()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            // the OPF is untrusted XML, so a DTD with an external entity must be rejected instead of being read and expanded
            string opfContent = """<?xml version="1.0" encoding="utf-8"?><!DOCTYPE package [<!ENTITY xxe SYSTEM "file:///etc/passwd">]><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>&xxe;</dc:title></metadata></package>""";
            string bookPath = WriteBookWithOpf(bookDirectory, opfContent);
            IMetadataProvider provider = CreateMetadataProvider();

            // Act
            MetadataDto? result = await provider.GetMetadataAsync(_bookMetadataLookupDtoFixture.Create(path: bookPath), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Null(((BookMetadataDto)result!).Title);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOpfContainsBillionLaughsStyleEntityExpansion_ShouldNotExpandTheEntities()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string opfContent = """<?xml version="1.0" encoding="utf-8"?><!DOCTYPE lolz [<!ENTITY lol "lol"><!ENTITY lol2 "&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;"><!ENTITY lol3 "&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;">]><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>&lol3;</dc:title></metadata></package>""";
            string bookPath = WriteBookWithOpf(bookDirectory, opfContent);
            IMetadataProvider provider = CreateMetadataProvider();

            // Act
            MetadataDto? result = await provider.GetMetadataAsync(_bookMetadataLookupDtoFixture.Create(path: bookPath), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Null(((BookMetadataDto)result!).Title);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOpfIsOversized_ShouldNotParseIt()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string oversizedContent = $"""<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>{new string('a', MAX_OPF_FILE_SIZE_BYTES + 100)}</dc:title></metadata></package>""";
            string bookPath = WriteBookWithOpf(bookDirectory, oversizedContent);
            IMetadataProvider provider = CreateMetadataProvider();

            // Act
            MetadataDto? result = await provider.GetMetadataAsync(_bookMetadataLookupDtoFixture.Create(path: bookPath), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Null(((BookMetadataDto)result!).Title);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOpfIsMalformedXml_ShouldNotExposeAnException()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = WriteBookWithOpf(bookDirectory, """<package xmlns="http://www.idpf.org/2007/opf"><metadata><dc:title>Unclosed""");
            IMetadataProvider provider = CreateMetadataProvider();

            // Act
            MetadataDto? result = await provider.GetMetadataAsync(_bookMetadataLookupDtoFixture.Create(path: bookPath), CancellationToken.None);

            // Assert
            // a malformed OPF must be swallowed and must not leak an exception to the caller
            Assert.NotNull(result);
            Assert.Null(((BookMetadataDto)result!).Title);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    /// <summary>
    /// Creates the Calibre metadata provider by registering the plugin services and resolving the keyed metadata provider.
    /// </summary>
    /// <returns>The created Calibre metadata provider.</returns>
    private static IMetadataProvider CreateMetadataProvider()
    {
        ServiceCollection services = new();
        new CalibrePlugin().RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetKeyedService<IMetadataProvider>(CalibrePlugin.s_pluginId)!;
    }

    /// <summary>
    /// Writes an OPF file with the given content into the book directory, and returns the path of the book.
    /// </summary>
    /// <param name="bookDirectory">The file system path of the directory the OPF file is written into.</param>
    /// <param name="opfContent">The content of the OPF file to write.</param>
    /// <returns>The file system path of the book.</returns>
    private static string WriteBookWithOpf(string bookDirectory, string opfContent)
    {
        string bookPath = Path.Combine(bookDirectory, "book.epub");
        File.WriteAllText(Path.Combine(bookDirectory, "metadata.opf"), opfContent);
        return bookPath;
    }

    /// <summary>
    /// Creates a unique temporary directory for the book.
    /// </summary>
    /// <returns>The file system path of the created temporary directory.</returns>
    private static string CreateTempDirectory()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), $"lumina-security-opf-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }
}
