#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="BookRatingEntity"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingEntityFixture
{
    private readonly Fixture _fixture;
    private readonly Random _random = new();
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingEntityFixture"/> class.
    /// </summary>
    public BookRatingEntityFixture()
    {
        _fixture = new Fixture();
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a complete book rating entity with all properties.
    /// </summary>
    /// <returns>The created book rating entity.</returns>
    public BookRatingEntity CreateComplete()
    {
        return new BookRatingEntity(
            _random.Next(1, 5),
            5,
            _fixture.Create<BookRatingSource>(),
            _random.Next(1, 1000)
        );
    }

    /// <summary>
    /// Creates a minimal book rating entity with only required properties.
    /// </summary>
    /// <returns>The created book rating entity.</returns>
    public BookRatingEntity CreateMinimal()
    {
        return new BookRatingEntity(
            _random.Next(1, 5),
            5,
            null,
            null
        );
    }

    /// <summary>
    /// Creates an invalid book rating entity.
    /// </summary>
    /// <returns>The created invalid book rating entity.</returns>
    public BookRatingEntity CreateInvalid()
    {
        return new BookRatingEntity(
            10, // invalid: value > maxValue
            5,
            _fixture.Create<BookRatingSource>(),
            _random.Next(1, 1000)
        );
    }
}
