#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Value Object uniquely identifying file system items.
/// </summary>
[DebuggerDisplay("{Path}")]
public sealed class FileSystemPathId : ValueObject
{
    /// <summary>
    /// Gets the value of this object.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemPathId"/> class.
    /// </summary>
    /// <param name="path">The path of the file system item.</param>
    private FileSystemPathId(string path)
    {
        Path = path;
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Path;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FileSystemPathId"/> class.
    /// </summary>
    /// <param name="path">The path of the file system item.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="FileSystemPathId"/> or an error message.
    /// </returns>
    public static Result<FileSystemPathId> Create(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Errors.FileSystemManagement.InvalidPath;
        return new FileSystemPathId(path);
    }
}
