#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Epub.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Epub.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="EpubReader"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpubReaderTests
{
    private readonly EpubReader _sut = new();

    [Fact]
    public void SupportedExtensions_WhenAccessed_ShouldOnlyContainEpub()
    {
        // Act
        IReadOnlyList<string> result = _sut.SupportedExtensions;

        // Assert
        Assert.Equal([".epub"], result);
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
