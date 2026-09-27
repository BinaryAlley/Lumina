#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookMetadataLookupDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataLookupDtoTests
{
    [Fact]
    public void Constructor_WhenOmittingOptionalParameters_ShouldUseNullDefaults()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();

        // Act
        BookMetadataLookupDto sut = new(libraryId, @"/media/books/dune.epub");

        // Assert
        Assert.Equal(libraryId, sut.LibraryId);
        Assert.Equal(@"/media/books/dune.epub", sut.Path);
        Assert.Null(sut.Isbn);
        Assert.Null(sut.OpenLibraryId);
        Assert.Null(sut.Title);
        Assert.Null(sut.Author);
        Assert.Null(sut.LanguageCode);
    }
}
