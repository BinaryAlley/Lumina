#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Pdf.Core;
using Lumina.Plugins.Pdf.Fixtures.Core.Pdf;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Pdf.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="PdfReader"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PdfReaderTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-pdf-reader-{Guid.NewGuid():N}");
    private readonly string _pdfPath;
    private readonly PdfReader _sut = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfReaderTests"/> class.
    /// </summary>
    public PdfReaderTests()
    {
        Directory.CreateDirectory(_temporaryDirectory);
        _pdfPath = Path.Combine(_temporaryDirectory, "minimal.pdf");
        TestPdfFileFactory.CreateMinimalPdf(_pdfPath);
    }

    [Fact]
    public void SupportedExtensions_WhenAccessed_ShouldOnlyContainPdf()
    {
        // Assert
        Assert.Equal([".pdf"], _sut.SupportedExtensions);
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
        ReadingDocumentDto result = await _sut.OpenAsync(_pdfPath, workingDirectory, shouldRenderPdfAsImages: false, CancellationToken.None);

        // Assert
        Assert.Equal("minimal", result.Title);
        Assert.Single(result.Spine);
    }

    [SupportedOSPlatform("windows")]
    [Fact]
    public async Task GetResourceAsync_WhenCalledForAPage_ShouldRenderThePageImage()
    {
        // Arrange
        string workingDirectory = Path.Combine(_temporaryDirectory, "work");

        // Act
        byte[] result = await _sut.GetResourceAsync(_pdfPath, workingDirectory, "page:1", CancellationToken.None);

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

