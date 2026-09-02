#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Pdf.Core.Pdf;
using System;
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
public class PdfDocumentParserTests
{
    [SupportedOSPlatform("windows")]
    [Fact]
    public void RenderResource_WhenResourceKeyDoesNotIdentifyAPage_ShouldThrowInvalidDataException()
    {
        // Arrange
        // The resource key is rejected before RenderResource opens the PDF, so the path never needs to exist.
        string missingPdfPath = "missing.pdf";

        // Act
        Action act = () => PdfDocumentParser.RenderResource(missingPdfPath, "not-a-page", CancellationToken.None);

        // Assert
        Assert.Throws<InvalidDataException>(act);
    }

    [SupportedOSPlatform("windows")]
    [Fact]
    public void RenderResource_WhenResourceKeyNamesAPageBeyondThePageCap_ShouldThrowInvalidDataException()
    {
        // Arrange
        // The page cap is enforced before RenderResource opens the PDF, so the path never needs to exist.
        string missingPdfPath = "missing.pdf";

        // Act
        Action act = () => PdfDocumentParser.RenderResource(missingPdfPath, "page:10001", CancellationToken.None);

        // Assert
        Assert.Throws<InvalidDataException>(act);
    }
}
