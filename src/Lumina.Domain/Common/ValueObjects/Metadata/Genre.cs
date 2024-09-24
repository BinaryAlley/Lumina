#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the genres of a media element.
/// </summary>
[DebuggerDisplay("{Name}")]
public class Genre : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name of the genre element of the media item.
    /// </summary>
    public string Name { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="Genre"/> class.
    /// </summary>
    /// <param name="name">The value representing this object.</param>
    private Genre(string name)
    {
        Name = name.Trim();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="Genre"/> class.
    /// </summary>
    /// <param name="name">The value of the genre.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="Genre"/>, or an error message.
    /// </returns>
    public static ErrorOr<Genre> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Errors.Metadata.GenreNameCannotBeEmpty;
        return new Genre(name);
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
