#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.Repositories.MusicLibrary.Specifications;
using Lumina.DataAccess.Core.UoW;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.MusicLibrary.Specifications;

/// <summary>
/// Contains integration tests for the <see cref="ArtistAlphaFilterSpecification"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistAlphaFilterSpecificationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LuminaDbContext _context;
    private readonly ArtistEntityFixture _artistEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistAlphaFilterSpecificationTests"/> class.
    /// </summary>
    public ArtistAlphaFilterSpecificationTests()
    {
        // The specification translates to the SQLite glob() function, so it can only be exercised against a real SQLite database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsNull_ShouldMatchEveryArtist()
    {
        // Arrange
        List<ArtistEntity> artists = SeedArtists("Alpha", "Beta", "1984");
        ArtistAlphaFilterSpecification specification = new(null, false);

        // Act
        List<ArtistEntity> result = [.. _context.Artists.Where(specification.ToExpression())];

        // Assert
        Assert.Equal(artists.Count, result.Count);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsALowercaseLetter_ShouldMatchOnlyArtistsWhoseNameStartsWithIt()
    {
        // Arrange
        SeedArtists("Foo Fighters", "Helloween", "The Beatles");
        ArtistAlphaFilterSpecification specification = new("f", false);

        // Act
        List<string> result = [.. _context.Artists.Where(specification.ToExpression()).Select(artist => artist.Name)];

        // Assert
        Assert.Equal(["Foo Fighters"], result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsAnUppercaseLetter_ShouldMatchArtistsWhoseNameStartsWithItsLowercase()
    {
        // Arrange
        SeedArtists("Foo Fighters", "Helloween");
        ArtistAlphaFilterSpecification specification = new("F", false);

        // Act
        List<string> result = [.. _context.Artists.Where(specification.ToExpression()).Select(artist => artist.Name)];

        // Assert
        Assert.Equal(["Foo Fighters"], result);
    }

    [Fact]
    public void ToExpression_WhenNoNameStartsWithTheAlphaKey_ShouldNotMatchAnyArtist()
    {
        // Arrange
        SeedArtists("Foo Fighters", "Helloween");
        ArtistAlphaFilterSpecification specification = new("z", false);

        // Act
        List<ArtistEntity> result = [.. _context.Artists.Where(specification.ToExpression())];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsTheNumberKey_ShouldMatchOnlyArtistsWhoseNameStartsWithADigit()
    {
        // Arrange
        SeedArtists("1984", "Alpha", "Beta");
        ArtistAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.NUMBER, false);

        // Act
        List<string> result = [.. _context.Artists.Where(specification.ToExpression()).Select(artist => artist.Name)];

        // Assert
        Assert.Equal(["1984"], result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsTheSymbolKey_ShouldMatchOnlyArtistsWhoseNameStartsWithASymbol()
    {
        // Arrange
        SeedArtists("!Important", "1984", "Alpha");
        ArtistAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.SYMBOL, false);

        // Act
        List<string> result = [.. _context.Artists.Where(specification.ToExpression()).Select(artist => artist.Name)];

        // Assert
        Assert.Equal(["!Important"], result);
    }

    [Fact]
    public void ToExpression_WhenIgnoringThePrefixAndNameStartsWithThe_ShouldMatchOnTheStrippedName()
    {
        // Arrange
        SeedArtists("The Beatles", "Adele");
        ArtistAlphaFilterSpecification specification = new("b", true);

        // Act
        List<string> result = [.. _context.Artists.Where(specification.ToExpression()).Select(artist => artist.Name)];

        // Assert
        Assert.Equal(["The Beatles"], result);
    }

    [Fact]
    public void ToExpression_WhenNotIgnoringThePrefixAndNameStartsWithThe_ShouldNotMatchOnTheStrippedName()
    {
        // Arrange
        SeedArtists("The Beatles", "Adele");
        ArtistAlphaFilterSpecification specification = new("b", false);

        // Act
        List<ArtistEntity> result = [.. _context.Artists.Where(specification.ToExpression())];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsTheNumberKeyAndNameStartsWithASymbol_ShouldNotMatchIt()
    {
        // Arrange
        SeedArtists("!Voyager", "1984", "Alpha");
        ArtistAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.NUMBER, false);

        // Act
        List<string> result = [.. _context.Artists.Where(specification.ToExpression()).Select(artist => artist.Name)];

        // Assert
        // Only the artist whose name starts with a digit matches; a symbol and a letter do not.
        Assert.Equal(["1984"], result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsTheSymbolKeyAndNameIsEmpty_ShouldMatchIt()
    {
        // Arrange
        SeedArtists(string.Empty);
        ArtistAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.SYMBOL, false);

        // Act
        List<ArtistEntity> result = [.. _context.Artists.Where(specification.ToExpression())];

        // Assert
        // An empty effective name is neither a letter nor a digit, so it is categorized as a symbol.
        Assert.Single(result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsALetterAndNameIsEmpty_ShouldNotMatchAnyArtist()
    {
        // Arrange
        SeedArtists(string.Empty);
        ArtistAlphaFilterSpecification specification = new("a", false);

        // Act
        List<ArtistEntity> result = [.. _context.Artists.Where(specification.ToExpression())];

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("a", "Alpha")] // the first letter of the alphabet
    [InlineData("b", "Bravo")] // a letter in the middle of the alphabet
    [InlineData("c", "Charlie")] // another letter in the middle of the alphabet
    public void ToExpression_WhenAlphaKeyIsALetter_ShouldMatchOnlyTheNamesStartingWithIt(string alphaKey, string matchingName)
    {
        // Arrange
        SeedArtists("Alpha", "Bravo", "Charlie");
        ArtistAlphaFilterSpecification specification = new(alphaKey, false);

        // Act
        List<string> result = [.. _context.Artists.Where(specification.ToExpression()).Select(artist => artist.Name)];

        // Assert
        Assert.Equal([matchingName], result);
    }

    /// <summary>
    /// Seeds artists with the provided names.
    /// </summary>
    /// <param name="names">The names of the artists to seed.</param>
    /// <returns>The seeded <see cref="ArtistEntity"/> instances.</returns>
    private List<ArtistEntity> SeedArtists(params string[] names)
    {
        List<ArtistEntity> entities = [];
        foreach (string name in names)
            entities.Add(_artistEntityFixture.Create(name: name, includeAlbums: false, includeContributors: false));
        _context.Artists.AddRange(entities);
        _context.SaveChanges();
        return entities;
    }

    /// <summary>
    /// Disposes the database context and its underlying connection.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
