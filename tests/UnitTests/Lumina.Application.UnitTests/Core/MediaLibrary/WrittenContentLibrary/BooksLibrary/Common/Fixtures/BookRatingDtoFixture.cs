#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="BookRatingDto"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingDtoFixture
{
    private readonly Fixture _fixture;
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingDtoFixture"/> class.
    /// </summary>
    public BookRatingDtoFixture()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// Creates a complete book rating DTO with all properties.
    /// </summary>
    /// <returns>The created book rating DTO.</returns>
    public BookRatingDto CreateComplete()
    {
        return new BookRatingDto(
            _random.Next(1, 5),
            5,
            _fixture.Create<BookRatingSource>(),
            _random.Next(1, 1000)
        );
    }

    /// <summary>
    /// Creates a minimal book rating DTO with only required properties.
    /// </summary>
    /// <returns>The created book rating DTO.</returns>
    public BookRatingDto CreateMinimal()
    {
        return new BookRatingDto(
            _random.Next(1, 5),
            5,
            null,
            null
        );
    }

    /// <summary>
    /// Creates an invalid book rating DTO.
    /// </summary>
    /// <returns>The created invalid book rating DTO.</returns>
    public BookRatingDto CreateInvalid()
    {
        return new BookRatingDto(
            10, // invalid: value > maxValue
            5,
            _fixture.Create<BookRatingSource>(),
            _random.Next(1, 1000)
        );
    }
}
