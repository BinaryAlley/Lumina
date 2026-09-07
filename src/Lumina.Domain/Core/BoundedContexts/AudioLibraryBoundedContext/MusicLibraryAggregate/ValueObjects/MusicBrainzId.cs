#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for a MusicBrainz identifier.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class MusicBrainzId : ValueObject
{
    /// <summary>
    /// Gets the unique identifier assigned by MusicBrainz.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicBrainzId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private MusicBrainzId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicBrainzId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MusicBrainzId"/> instance.</param>
    /// <returns>The created <see cref="MusicBrainzId"/> instance.</returns>
    public static Result<MusicBrainzId> Create(Guid value)
    {
        return new MusicBrainzId(value);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicBrainzId"/> class, from a <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MusicBrainzId"/> instance.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MusicBrainzId"/>, or an error message.
    /// </returns>
    public static Result<MusicBrainzId> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Errors.Music.MusicBrainzIdInvalidFormat;
        if (!Guid.TryParse(value, out Guid parsedValue))
            return Errors.Music.MusicBrainzIdInvalidFormat;

        return new MusicBrainzId(parsedValue);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
