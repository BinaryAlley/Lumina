#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;

/// <summary>
/// Value object uniquely identifying path segments.
/// </summary>
[DebuggerDisplay("{Name}")]
public class PathSegment : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name of the path segment.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the value indicating if the current path segment is a file system directory or not.
    /// </summary>
    public bool IsDirectory { get; }

    /// <summary>
    /// Gets the value indicating if the current path segment is a file system drive or not.
    /// </summary>
    public bool IsDrive { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="PathSegment"/> class.
    /// </summary>
    /// <param name="name">The name of the path segment.</param>
    /// <param name="isDirectory">Value indicating if the current path segment is a file system directory or not.</param>
    /// <param name="isDrive">Value indicating if the current path segment is a file system drive or not.</param>
    private PathSegment(string name, bool isDirectory, bool isDrive)
    {
        Name = name;
        IsDirectory = isDirectory;
        IsDrive = isDrive;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="PathSegment"/> class.
    /// </summary>
    /// <param name="name">The name of the path segment.</param>
    /// <param name="isDirectory">Value indicating if the current path segment is a file system directory or not.</param>
    /// <param name="isDrive">Value indicating if the current path segment is a file system drive or not.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="PathSegment"/> or an error message.
    /// </returns>
    public static ErrorOr<PathSegment> Create(string name, bool isDirectory, bool isDrive)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.FileManagement.NameCannotBeEmpty;
        return new PathSegment(name, isDirectory, isDrive);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return IsDirectory;
        yield return IsDrive;
    }
    #endregion
}
