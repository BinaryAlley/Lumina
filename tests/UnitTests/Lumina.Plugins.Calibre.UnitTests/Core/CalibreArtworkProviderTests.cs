#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Calibre.Core;
using Lumina.Plugins.Calibre.Fixtures.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Calibre.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="CalibreArtworkProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CalibreArtworkProviderTests
{
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();
    private readonly TestMetadataLookupFixture _testMetadataLookupFixture = new();
    private readonly CalibreArtworkProvider _sut = new();

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
    public async Task GetArtworkAsync_WhenLookupIsNotABookLookup_ShouldReturnNull()
    {
        // Arrange
        MetadataLookupDto lookup = _testMetadataLookupFixture.Create();
        IArtworkProvider baseProvider = _sut;

        // Act
        ArtworkDto? result = await baseProvider.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenLookupPathIsEmpty_ShouldReturnNull()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "   ");

        // Act
        ArtworkDto? result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
