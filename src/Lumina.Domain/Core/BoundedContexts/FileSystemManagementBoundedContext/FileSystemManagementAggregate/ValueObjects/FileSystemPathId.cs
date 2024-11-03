#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;

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

    /// <inheritdoc />
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Path;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FileSystemPathId"/> class.
    /// </summary>
    /// <param name="path">The path of the file system item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="FileSystemPathId"/> or an error message.
    /// </returns>
    public static ErrorOr<FileSystemPathId> Create(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Errors.FileSystemManagement.InvalidPath;
        return new FileSystemPathId(path);
    }
}
