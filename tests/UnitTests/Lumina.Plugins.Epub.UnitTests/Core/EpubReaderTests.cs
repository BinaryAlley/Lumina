#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Epub.Core;
using Lumina.Plugins.Epub.Fixtures.Core.Epub;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Epub.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="EpubReader"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpubReaderTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-epub-reader-{Guid.NewGuid():N}");
    private readonly string _epubPath;
    private readonly EpubReader _sut = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EpubReaderTests"/> class.
    /// </summary>
    public EpubReaderTests()
    {
        Directory.CreateDirectory(_temporaryDirectory);
        _epubPath = Path.Combine(_temporaryDirectory, "minimal.epub");
        TestEpubFileFactory.CreateMinimalEpub(_epubPath);
    }

    [Fact]
    public void SupportedExtensions_WhenAccessed_ShouldOnlyContainEpub()
    {
        // Assert
        Assert.Equal([".epub"], _sut.SupportedExtensions);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenAccessed_ShouldContainEBookAndBook()
    {
        // Assert
        Assert.Equal([LibraryType.EBook, LibraryType.Book], _sut.SupportedLibraryTypes);
    }

    [Fact]
    public async Task OpenAsync_WhenCalled_ShouldReturnTheParsedDocument()
    {
        // Arrange
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        ReadingDocumentDto result = await _sut.OpenAsync(_epubPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.Equal("Minimal EPUB Book", result.Title);
        Assert.Equal(2, result.Spine.Count);
    }

    [Fact]
    public async Task GetResourceAsync_WhenResourceWasExtracted_ShouldReturnItsBytes()
    {
        // Arrange
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");
        ReadingDocumentDto document = await _sut.OpenAsync(_epubPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);
        string resourceKey = document.Resources.Keys.Single();

        // Act
        byte[] result = await _sut.GetResourceAsync(_epubPath, workingDirectory, resourceKey, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
    }
}

