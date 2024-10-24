#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;

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
        result.Should().NotBeNull();
        result.Value.Should().Be(isbn.Value);
        result.Format.Should().Be(IsbnFormat.Isbn10);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingIsbn13_ShouldMapCorrectly()
    {
        // Arrange
        Isbn isbn = _isbnFixture.CreateIsbn13();

        // Act
        IsbnEntity result = isbn.ToRepositoryEntity();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(isbn.Value);
        result.Format.Should().Be(IsbnFormat.Isbn13);
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
        results.Should().NotBeNull();
        results.Should().HaveCount(isbns.Count);

        List<IsbnEntity> resultList = results.ToList();
        for (int i = 0; i < isbns.Count; i++)
        {
            resultList[i].Value.Should().Be(isbns[i].Value);
            resultList[i].Format.Should().Be(isbns[i].Format);
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
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }
}
