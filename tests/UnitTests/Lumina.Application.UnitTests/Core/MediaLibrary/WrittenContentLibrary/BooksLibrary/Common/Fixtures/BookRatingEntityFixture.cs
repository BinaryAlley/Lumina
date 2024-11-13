#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="BookRatingEntity"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingEntityFixture
{
    private readonly Random _random = new();
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingEntityFixture"/> class.
    /// </summary>
    public BookRatingEntityFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a book rating entity.
    /// </summary>
    /// <returns>The created book rating entity.</returns>
    public BookRatingEntity CreateBookRating()
    {
        return new BookRatingEntity(
            _random.Next(1, 5),
            5,
            _faker.PickRandom<BookRatingSource>(),
            _random.Next(1, 1000)
        );
    }

    /// <summary>
    /// Creates an invalid book rating entity.
    /// </summary>
    /// <returns>The created invalid book rating entity.</returns>
    public BookRatingEntity CreateInvalidBookRating()
    {
        return new BookRatingEntity(
            null,
            null,
            null,
            null
        );
    }
}
