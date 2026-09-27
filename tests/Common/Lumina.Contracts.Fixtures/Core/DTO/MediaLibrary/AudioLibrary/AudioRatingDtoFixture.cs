#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;

/// <summary>
/// Fixture class for the <see cref="AudioRatingDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingDtoFixture
{
    private readonly Fixture _fixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AudioRatingDto"/>.
    /// </summary>
    /// <param name="value">Optional. The rating value.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value.</param>
    /// <param name="source">Optional. The rating source.</param>
    /// <param name="voteCount">Optional. The number of votes.</param>
    /// <param name="includeValue">Whether the rating value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMaxValue">Whether the maximum possible rating value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSource">Whether the rating source should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeVoteCount">Whether the number of votes should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AudioRatingDto"/>.</returns>
    public AudioRatingDto Create(
        decimal? value = null,
        decimal? maxValue = null,
        AudioRatingSource? source = null,
        int? voteCount = null,
        bool includeValue = true,
        bool includeMaxValue = true,
        bool includeSource = true,
        bool includeVoteCount = true)
    {
        return new AudioRatingDto(
            includeValue ? (value ?? Random.Shared.Next(1, 5)) : null,
            includeMaxValue ? (maxValue ?? 5) : null,
            includeSource ? (source ?? _fixture.Create<AudioRatingSource>()) : null,
            includeVoteCount ? (voteCount ?? Random.Shared.Next(1, 1000)) : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="AudioRatingDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AudioRatingDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
