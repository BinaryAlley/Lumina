#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the tags of a media element.
/// </summary>
[DebuggerDisplay("{Name}")]
public class Tag : ValueObject
{
    /// <summary>
    /// Gets the name of the tag element of the media item.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tag"/> class.
    /// </summary>
    /// <param name="name">The value representing this object.</param>
    private Tag(string name)
    {
        Name = name.Trim();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Tag"/> class.
    /// </summary>
    /// <param name="name">The value of the tag.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="Tag"/>, or an error message.
    /// </returns>
    public static ErrorOr<Tag> Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Errors.Metadata.TagNameCannotBeEmpty;
        return new Tag(name);
    }

    /// <inheritdoc/>
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
