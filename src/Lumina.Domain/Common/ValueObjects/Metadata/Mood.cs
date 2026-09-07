#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the moods of a media element.
/// </summary>
[DebuggerDisplay("{Name}")]
public class Mood : ValueObject
{
    /// <summary>
    /// Gets the name of the mood element of the media item.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mood"/> class.
    /// </summary>
    /// <param name="name">The value representing this object.</param>
    private Mood(string name)
    {
        Name = name.Trim();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Mood"/> class.
    /// </summary>
    /// <param name="name">The value of the mood.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Mood"/>, or an error message.
    /// </returns>
    public static Result<Mood> Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Errors.Metadata.MoodNameCannotBeEmpty;
        return new Mood(name);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        return Name;
    }
}
