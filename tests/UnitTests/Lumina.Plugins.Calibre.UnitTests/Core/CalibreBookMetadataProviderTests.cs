#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Calibre.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Calibre.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="CalibreBookMetadataProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CalibreBookMetadataProviderTests
{
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();
    private readonly CalibreBookMetadataProvider _sut = new();

    private const string OPF_DOCUMENT = """<?xml version="1.0" encoding="utf-8"?><package xmlns="http://www.idpf.org/2007/opf" version="2.0"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:opf="http://www.idpf.org/2007/opf"><dc:title>Test Book Title</dc:title><dc:creator opf:role="aut">Test Author</dc:creator></metadata></package>""";

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("Calibre", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnBookAndEBook()
    {
        // Act
        IReadOnlyList<LibraryType> result = _sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(LibraryType.Book, result[0]);
        Assert.Equal(LibraryType.EBook, result[1]);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnFalse()
    {
        // Act
        bool result = _sut.RequiresWebAccess;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOpfFileExistsNextToTheBook_ShouldReturnTheMappedMetadata()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            File.WriteAllText(Path.Combine(bookDirectory, "metadata.opf"), OPF_DOCUMENT);
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            BookMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Book Title", result!.Title);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetMetadataAsync_WhenOpfFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            BookMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetMetadataAsync_WhenLookupPathIsEmpty_ShouldReturnNull()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "   ");

        // Act
        BookMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenOpfFileExistsNextToTheBook_ShouldReturnASingleMetadataCandidate()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            File.WriteAllText(Path.Combine(bookDirectory, "metadata.opf"), OPF_DOCUMENT);
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            IReadOnlyList<BookMetadataDto> result = await _sut.GetSearchResultsAsync(lookup, CancellationToken.None);

            // Assert
            BookMetadataDto metadata = Assert.IsType<BookMetadataDto>(Assert.Single(result));
            Assert.Equal("Test Book Title", metadata.Title);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenOpfFileDoesNotExist_ShouldReturnAnEmptyList()
    {
        // Arrange
        string bookDirectory = CreateTempDirectory();
        try
        {
            string bookPath = Path.Combine(bookDirectory, "book.epub");
            BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: bookPath);

            // Act
            IReadOnlyList<BookMetadataDto> result = await _sut.GetSearchResultsAsync(lookup, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            Directory.Delete(bookDirectory, true);
        }
    }

    /// <summary>
    /// Creates a unique temporary directory for the book.
    /// </summary>
    /// <returns>The file system path of the created temporary directory.</returns>
    private static string CreateTempDirectory()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), $"lumina-calibre-metadata-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }
}
