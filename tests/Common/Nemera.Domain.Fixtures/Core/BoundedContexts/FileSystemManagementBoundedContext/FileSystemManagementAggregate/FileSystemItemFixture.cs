#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;

/// <summary>
/// Test-support class that allows the abstract <see cref="FileSystemItem"/> type to be instantiated in tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemFixture : FileSystemItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemItemFixture"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the file system path.</param>
    /// <param name="name">The name of the file system item.</param>
    /// <param name="fileSystemItemType">The type of the file system item.</param>
    public FileSystemItemFixture(FileSystemPathId id, string name, FileSystemItemType fileSystemItemType)
        : base(id, name, fileSystemItemType)
    {
    }
}
