#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookRatingDto"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingDtoFixture
{
    private readonly Fixture _fixture = new();
    private readonly Random _random = new();

    /// <summary>
    /// Creates a <see cref="BookRatingDto"/>.
    /// </summary>
    /// <param name="value">Optional. The rating value.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value.</param>
    /// <param name="source">Optional. The rating source.</param>
    /// <param name="voteCount">Optional. The number of votes.</param>
    /// <param name="includeOptionalProperties">Whether the optional source and vote count should be included.</param>
    /// <returns>The created <see cref="BookRatingDto"/>.</returns>
    public BookRatingDto Create(
        decimal? value = null,
        decimal? maxValue = null,
        BookRatingSource? source = null,
        int? voteCount = null,
        bool includeOptionalProperties = true)
    {
        return new BookRatingDto(
            value ?? _random.Next(1, 5),
            maxValue ?? 5,
            includeOptionalProperties ? (source ?? _fixture.Create<BookRatingSource>()) : null,
            includeOptionalProperties ? (voteCount ?? _random.Next(1, 1000)) : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="BookRatingDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<BookRatingDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
