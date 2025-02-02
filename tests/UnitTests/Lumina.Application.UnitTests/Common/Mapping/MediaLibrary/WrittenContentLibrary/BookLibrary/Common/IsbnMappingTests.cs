#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="IsbnMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnMappingTests
{
    private readonly IsbnFixture _isbnFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsbnMappingTests"/> class.
    /// </summary>
    public IsbnMappingTests()
    {
        _isbnFixture = new IsbnFixture();
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingIsbn10_ShouldMapCorrectly()
    {
        // Arrange
        Isbn isbn = _isbnFixture.CreateIsbn10();

        // Act
        IsbnEntity result = isbn.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(isbn.Value, result.Value);
        Assert.Equal(IsbnFormat.Isbn10, result.Format);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingIsbn13_ShouldMapCorrectly()
    {
        // Arrange
        Isbn isbn = _isbnFixture.CreateIsbn13();

        // Act
        IsbnEntity result = isbn.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(isbn.Value, result.Value);
        Assert.Equal(IsbnFormat.Isbn13, result.Format);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleIsbns_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Isbn> isbns =
        [
            _isbnFixture.CreateIsbn10(),
            _isbnFixture.CreateIsbn13(),
            _isbnFixture.CreateIsbn10()
        ];

        // Act
        IEnumerable<IsbnEntity> results = isbns.ToRepositoryEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(isbns.Count, results.Count());

        List<IsbnEntity> resultList = results.ToList();
        for (int i = 0; i < isbns.Count; i++)
        {
            Assert.Equal(isbns[i].Value, resultList[i].Value);
            Assert.Equal(isbns[i].Format, resultList[i].Format);
        }
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        List<Isbn> isbns = [];

        // Act
        IEnumerable<IsbnEntity> results = isbns.ToRepositoryEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }
}
