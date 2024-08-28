#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Common.Factory;

/// <summary>
/// Contains unit tests for the <see cref="RepositoryDictionary"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RepositoryDictionaryTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryDictionaryTests"/> class.
    /// </summary>
    public RepositoryDictionaryTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Add_WhenValidRepositoryItemIsAdded_ShouldIncreaseCount()
    {
        // Arrange
        var bookRepository = _fixture.Create<BookRepository>();
        var repositoryDictionary = new RepositoryDictionary();

        // Act
        repositoryDictionary.Add<IBookRepository>(bookRepository);

        // Assert
        repositoryDictionary.Count.Should().Be(1);
    }

    [Fact]
    public void Clear_WhenCalled_ShouldRemoveAllItems()
    {
        // Arrange
        var bookRepository = _fixture.Create<BookRepository>();
        var repositoryDictionary = new RepositoryDictionary();
        repositoryDictionary.Add<IBookRepository>(bookRepository);

        // Act
        repositoryDictionary.Clear();

        // Assert
        repositoryDictionary.Count.Should().Be(0);
    }

    [Fact]
    public void Get_WhenRepositoryExists_ShouldReturnCorrectRepository()
    {
        // Arrange
        var expected = _fixture.Create<BookRepository>();
        var repositoryDictionary = new RepositoryDictionary();
        repositoryDictionary.Add<IBookRepository>(expected);

        // Act
        var actual = repositoryDictionary.Get<IBookRepository>(typeof(BookRepository));

        // Assert
        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Add_WhenNullValueIsProvided_ShouldThrowArgumentException()
    {
        // Arrange
        var repositoryDictionary = new RepositoryDictionary();

        // Act & Assert
        Action act = () => repositoryDictionary.Add<IBookRepository>(null!);
        act.Should().Throw<ArgumentException>().WithMessage("Value cannot be null!");
    }

    [Fact]
    public void Add_WhenDuplicateValueIsAdded_ShouldThrowArgumentException()
    {
        // Arrange
        var bookRepository = _fixture.Create<BookRepository>();
        var repositoryDictionary = new RepositoryDictionary();
        repositoryDictionary.Add<IBookRepository>(bookRepository);

        // Act & Assert
        Action act = () => repositoryDictionary.Add<IBookRepository>(bookRepository);
        act.Should().Throw<ArgumentException>().WithMessage("Duplicate values are not allowed!");
    }

    [Fact]
    public void Add_WhenNonRepositoryValueIsAdded_ShouldThrowArgumentException()
    {
        // Arrange
        var nonRepository = new object();
        var repositoryDictionary = new RepositoryDictionary();

        // Act & Assert
        Action act = () => repositoryDictionary.Add(nonRepository);
        act.Should().Throw<ArgumentException>().WithMessage("Value must implement IRepository interface!");
    }
    #endregion
}