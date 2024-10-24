#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.FileSystem.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileSystemItem"/> class.
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
