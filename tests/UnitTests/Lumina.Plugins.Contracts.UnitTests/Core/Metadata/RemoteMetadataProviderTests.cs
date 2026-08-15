#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.UnitTests.Common.Fakes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Core.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="IRemoteMetadataProvider"/> contracts.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoteMetadataProviderTests
{
    private readonly IRemoteMetadataProvider _sut = new FakeBookMetadataProvider();

    [Fact]
    public async Task GetMetadataAsync_WhenCalledThroughNonGenericInterface_ShouldReturnTypedMetadata()
    {
        // Arrange
        MetadataLookupDto lookup = new BookMetadataLookupDto(
            LibraryId: Guid.NewGuid(),
            Path: "/books/the-fellowship-of-the-ring.epub",
            Title: "The Fellowship of the Ring");

        // Act
        MetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        BookMetadataDto bookMetadata = Assert.IsType<BookMetadataDto>(result);
        Assert.Equal("The Fellowship of the Ring", bookMetadata.Title);
        Assert.Equal("3", bookMetadata.GoodreadsId);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenNoMetadataFound_ShouldReturnNull()
    {
        // Arrange
        MetadataLookupDto lookup = new BookMetadataLookupDto(
            LibraryId: Guid.NewGuid(),
            Path: "/books/unknown.epub");

        // Act
        MetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenCalledThroughNonGenericInterface_ShouldReturnTypedResults()
    {
        // Arrange
        MetadataLookupDto lookup = new BookMetadataLookupDto(
            LibraryId: Guid.NewGuid(),
            Path: "/books/the-fellowship-of-the-ring.epub",
            Title: "The Fellowship of the Ring");

        // Act
        IReadOnlyList<MetadataDto> result = await _sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Single(result);
        BookMetadataDto bookMetadata = Assert.IsType<BookMetadataDto>(result[0]);
        Assert.Equal("The Fellowship of the Ring", bookMetadata.Title);
    }
}
