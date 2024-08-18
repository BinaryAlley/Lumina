#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the tags of a media element.
/// </summary>
[DebuggerDisplay("{Name}")]
public class Tag : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name of the tag element of the media item.
    /// </summary>
    public string Name { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="name">The value representing this object.</param>
    private Tag(string name)
    {
        Name = name.Trim();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of <see cref="Tag"/>.
    /// </summary>
    /// <param name="name">The value of the tag.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing either a successfully created <see cref="Tag"/> or an error message.
    /// </returns>
    public static ErrorOr<Tag> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Errors.Metadata.TagCannotBeEmpty;
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
    #endregion
}