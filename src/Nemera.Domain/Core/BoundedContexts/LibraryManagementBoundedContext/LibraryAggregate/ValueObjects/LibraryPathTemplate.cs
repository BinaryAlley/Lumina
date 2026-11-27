#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the ordered sequence of parts that make up the structure of a media library on disk.
/// </summary>
public class LibraryPathTemplate : ValueObject
{
    private readonly List<LibraryPathPart> _parts;

    /// <summary>
    /// Gets the ordered parts that make up the path template.
    /// </summary>
    public IReadOnlyList<LibraryPathPart> Parts => _parts.AsReadOnly();

    /// <summary>
    /// Gets whether the template carries no parts, which means no path derived metadata can be obtained from it.
    /// </summary>
    public bool IsEmpty => _parts.Count == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPathTemplate"/> class.
    /// </summary>
    /// <param name="parts">The ordered parts that make up the path template.</param>
    private LibraryPathTemplate(List<LibraryPathPart> parts)
    {
        _parts = parts;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LibraryPathTemplate"/> class.
    /// </summary>
    /// <param name="parts">The ordered parts that make up the path template.</param>
    /// <returns>The created path template.</returns>
    public static LibraryPathTemplate Create(IEnumerable<LibraryPathPart> parts)
    {
        return new LibraryPathTemplate([.. parts]);
    }

    /// <summary>
    /// Creates an empty path template, used by the library types that have no path part catalog yet.
    /// </summary>
    /// <returns>The created empty path template.</returns>
    public static LibraryPathTemplate Empty()
    {
        return new LibraryPathTemplate([]);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        return _parts;
    }
}
