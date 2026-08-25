#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Plugins.Calibre.Core;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Plugins.Calibre;

/// <summary>
/// Contains security tests for the <see cref="CalibreArtworkProvider"/> class, exercised through the public artwork provider surface.
/// </summary>
[ExcludeFromCodeCoverage]
public class CalibreArtworkProviderSecurityTests
{
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();

    [Fact]
    public async Task GetArtworkAsync_WhenCoverHrefIsABareFileName_ShouldReturnTheCoverInsideTheBookDirectory()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = WriteBookWithCoverReference(bookDirectory, "cover.jpg");
            File.WriteAllText(Path.Combine(bookDirectory, "cover.jpg"), "image-bytes");
            IArtworkProvider provider = CreateArtworkProvider();

            // Act
            ArtworkDto? result = await provider.GetArtworkAsync(_bookMetadataLookupDtoFixture.Create(path: bookPath), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Path.Combine(bookDirectory, "cover.jpg"), result!.LocalPath);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Theory]
    [InlineData("../cover.jpg")] // parent directory escape
    [InlineData("../../cover.jpg")] // multiple parent directory escape
    [InlineData("sub/cover.jpg")] // forward slash path traversal
    [InlineData("sub\\cover.jpg")] // backslash path traversal
    [InlineData(@"C:\Windows\System32\drivers\etc\hosts")] // absolute Windows path
    [InlineData("/etc/passwd")] // absolute Unix path
    [InlineData("http://evil.example/cover.jpg")] // remote URL
    [InlineData("file:///etc/passwd")] // file URI
    public async Task GetArtworkAsync_WhenCoverHrefEscapesTheBookDirectory_ShouldReturnNull(string coverHref)
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = WriteBookWithCoverReference(bookDirectory, coverHref);
            IArtworkProvider provider = CreateArtworkProvider();

            // Act
            ArtworkDto? result = await provider.GetArtworkAsync(_bookMetadataLookupDtoFixture.Create(path: bookPath), CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    /// <summary>
    /// Creates the Calibre artwork provider by registering the plugin services and resolving the keyed artwork provider.
    /// </summary>
    /// <returns>The created Calibre artwork provider.</returns>
    private static IArtworkProvider CreateArtworkProvider()
    {
        ServiceCollection services = new();
        new CalibrePlugin().RegisterServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetKeyedService<IArtworkProvider>(CalibrePlugin.s_pluginId)!;
    }

    /// <summary>
    /// Writes an OPF file that references the given cover into the book directory, and returns the path of the book.
    /// </summary>
    /// <param name="bookDirectory">The file system path of the directory the OPF file is written into.</param>
    /// <param name="coverHref">The href of the cover the OPF file references.</param>
    /// <returns>The file system path of the book.</returns>
    private static string WriteBookWithCoverReference(string bookDirectory, string coverHref)
    {
        string opf = $$"""<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>Test</dc:title></metadata><guide><reference type="cover" href="{{coverHref}}"/></guide></package>""";
        string bookPath = Path.Combine(bookDirectory, "book.epub");
        File.WriteAllText(Path.Combine(bookDirectory, "metadata.opf"), opf);
        return bookPath;
    }

    /// <summary>
    /// Creates a unique temporary directory for the book.
    /// </summary>
    /// <returns>The file system path of the created temporary directory.</returns>
    private static string CreateTempDirectory()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), $"lumina-security-artwork-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }
}
