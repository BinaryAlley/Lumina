#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Plugins.Pdf.Core;
using Lumina.Plugins.Pdf.Fixtures.Core.Pdf;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Pdf.IntegrationTests.Core;

/// <summary>
/// Contains integration tests for the <see cref="PdfReader"/> class.
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

