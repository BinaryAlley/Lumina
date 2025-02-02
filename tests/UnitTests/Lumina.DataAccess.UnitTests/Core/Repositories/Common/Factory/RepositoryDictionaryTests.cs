#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Common.Factory;

/// <summary>
/// Contains unit tests for the <see cref="RepositoryDictionary"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RepositoryDictionaryTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryDictionaryTests"/> class.
    /// </summary>
    public RepositoryDictionaryTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void Add_WhenValidRepositoryItemIsAdded_ShouldIncreaseCount()
    {
        // Arrange
        BookRepository bookRepository = _fixture.Create<BookRepository>();
        RepositoryDictionary repositoryDictionary = new();

        // Act
        repositoryDictionary.Add<IBookRepository>(bookRepository);

        // Assert
        Assert.Equal(1, repositoryDictionary.Count);
    }

    [Fact]
    public void Clear_WhenCalled_ShouldRemoveAllItems()
    {
        // Arrange
        BookRepository bookRepository = _fixture.Create<BookRepository>();
        RepositoryDictionary repositoryDictionary = new();
        repositoryDictionary.Add<IBookRepository>(bookRepository);

        // Act
        repositoryDictionary.Clear();

        // Assert
        Assert.Equal(0, repositoryDictionary.Count);
    }

    [Fact]
    public void Get_WhenRepositoryExists_ShouldReturnCorrectRepository()
    {
        // Arrange
        BookRepository expected = _fixture.Create<BookRepository>();
        RepositoryDictionary repositoryDictionary = new();
        repositoryDictionary.Add<IBookRepository>(expected);

        // Act
        IBookRepository actual = repositoryDictionary.Get<IBookRepository>(typeof(BookRepository));

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public void Add_WhenNullValueIsProvided_ShouldThrowArgumentException()
    {
        // Arrange
        RepositoryDictionary repositoryDictionary = new();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => repositoryDictionary.Add<IBookRepository>(null!));
    }

    [Fact]
    public void Add_WhenDuplicateValueIsAdded_ShouldThrowArgumentException()
    {
        // Arrange
        BookRepository bookRepository = _fixture.Create<BookRepository>();
        RepositoryDictionary repositoryDictionary = new();
        repositoryDictionary.Add<IBookRepository>(bookRepository);

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => repositoryDictionary.Add<IBookRepository>(bookRepository));
        Assert.Equal("Duplicate values are not allowed!", exception.Message);
    }

    [Fact]
    public void Add_WhenNonRepositoryValueIsAdded_ShouldThrowArgumentException()
    {
        // Arrange
        object nonRepository = new();
        RepositoryDictionary repositoryDictionary = new();

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => repositoryDictionary.Add(nonRepository));
        Assert.Equal("Value must implement IRepository interface!", exception.Message);
    }
}
