#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileSystemItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemFixture : FileSystemItem
{
    #region ====================================================================== CTOR =====================================================================================
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
    #endregion
}
