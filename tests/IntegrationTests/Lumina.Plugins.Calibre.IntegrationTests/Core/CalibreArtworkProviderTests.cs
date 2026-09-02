#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Plugins.Calibre.Core;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Calibre.IntegrationTests.Core;

/// <summary>
/// Contains integration tests for the <see cref="CalibreArtworkProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CalibreArtworkProviderTests
{
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();
    private readonly CalibreArtworkProvider _sut = new();

    [Fact]
    public async Task GetArtworkAsync_WhenCoverHrefIsABareFileName_ShouldReturnTheLocalCoverPath()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            WriteOpf(bookDirectory, coverHref: "cover.jpg");
            File.WriteAllText(Path.Combine(bookDirectory, "cover.jpg"), "image-bytes");
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            ArtworkDto? result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Path.Combine(bookDirectory, "cover.jpg"), result!.LocalPath);
            Assert.Null(result.RemoteUrl);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetArtworkAsync_WhenCoverHrefIsAnAbsolutePath_ShouldReturnNull()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            WriteOpf(bookDirectory, coverHref: @"C:\Windows\System32\drivers\etc\hosts");
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            ArtworkDto? result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Theory]
    [InlineData("../cover.jpg")] // parent directory escape
    [InlineData("sub/cover.jpg")] // path with a separator
    [InlineData("sub\\cover.jpg")] // path with a backslash separator
    public async Task GetArtworkAsync_WhenCoverHrefContainsPathTraversalOrSeparators_ShouldReturnNull(string coverHref)
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            WriteOpf(bookDirectory, coverHref);
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            ArtworkDto? result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetArtworkAsync_WhenCoverFileIsMissing_ShouldReturnNull()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            WriteOpf(bookDirectory, coverHref: "cover.jpg");
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            ArtworkDto? result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetArtworkAsync_WhenOpfFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            ArtworkDto? result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetArtworkAsync_WhenOpfHasNoCoverReference_ShouldReturnNull()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            File.WriteAllText(Path.Combine(bookDirectory, "metadata.opf"), """<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>Test</dc:title></metadata></package>""");
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            ArtworkDto? result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    /// <summary>
    /// Writes an OPF file that references the given cover into the book directory.
    /// </summary>
    /// <param name="bookDirectory">The file system path of the directory the OPF file is written into.</param>
    /// <param name="coverHref">The href of the cover the OPF file references.</param>
    private static void WriteOpf(string bookDirectory, string coverHref)
    {
        string opf = $$"""<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>Test</dc:title></metadata><guide><reference type="cover" href="{{coverHref}}"/></guide></package>""";
        File.WriteAllText(Path.Combine(bookDirectory, "metadata.opf"), opf);
    }

    /// <summary>
    /// Creates a unique temporary directory for the book.
    /// </summary>
    /// <returns>The file system path of the created temporary directory.</returns>
    private static string CreateTempDirectory()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), $"lumina-calibre-artwork-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }
}
