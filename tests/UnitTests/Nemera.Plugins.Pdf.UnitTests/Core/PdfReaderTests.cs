#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Pdf.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Pdf.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="PdfReader"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PdfReaderTests
{
    private readonly PdfReader _sut = new();

    [Fact]
    public void SupportedExtensions_WhenAccessed_ShouldOnlyContainPdf()
    {
        // Act
        IReadOnlyList<string> result = _sut.SupportedExtensions;

        // Assert
        Assert.Equal([".pdf"], result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenAccessed_ShouldContainEBookAndBook()
    {
        // Act
        IReadOnlyList<LibraryType> result = _sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.EBook, LibraryType.Book], result);
    }
}
