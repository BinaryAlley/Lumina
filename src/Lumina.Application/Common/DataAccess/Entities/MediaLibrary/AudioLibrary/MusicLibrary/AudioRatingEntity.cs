#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for the rating of an audio media element.
/// </summary>
/// <param name="Value">The rating value of the audio media element.</param>
/// <param name="MaxValue">The maximum possible rating value of the audio media element.</param>
/// <param name="Source">The source of the audio rating (e.g., a specific website or platform).</param>
/// <param name="VoteCount">The number of votes that contributed to the audio rating.</param>
[DebuggerDisplay("{Value}/{MaxValue}")]
public record AudioRatingEntity(
    decimal? Value,
    decimal? MaxValue,
    AudioRatingSource? Source,
    int? VoteCount
) : RatingEntity(Value, MaxValue, VoteCount);
