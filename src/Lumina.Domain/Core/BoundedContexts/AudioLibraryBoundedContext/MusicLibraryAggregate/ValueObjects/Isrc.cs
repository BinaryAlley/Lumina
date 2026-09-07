#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the ISRC (International Standard Recording Code) of a track.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class Isrc : ValueObject
{
    private static readonly Regex s_isrcRegex = new(@"^[A-Z]{2}[A-Z0-9]{3}[0-9]{7}$", RegexOptions.Compiled);

    /// <summary>
    /// Gets the value of the ISRC.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Isrc"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private Isrc(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Isrc"/> class.
    /// </summary>
    /// <param name="value">The value of the ISRC.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Isrc"/>, or an error message.
    /// </returns>
    public static Result<Isrc> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Errors.Music.IsrcValueCannotBeEmpty;

        string cleanedValue = value.Trim().ToUpperInvariant();
        if (!s_isrcRegex.IsMatch(cleanedValue))
            return Errors.Music.IsrcInvalidFormat;

        return new Isrc(cleanedValue);
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
