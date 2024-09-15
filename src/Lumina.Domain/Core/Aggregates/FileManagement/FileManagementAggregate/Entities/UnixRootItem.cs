#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;

/// <summary>
/// Entity representing a file system root item on Unix platforms.
/// </summary>
[DebuggerDisplay("{Id.Path}")]
public sealed class UnixRootItem : FileSystemItem
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly List<FileSystemItem> _items = [];
    private const string PATH = "/";
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the collection of file system items that are children to the current file system root entity.
    /// </summary>
    public IReadOnlyCollection<FileSystemItem> Items
    {
        get { return _items.AsReadOnly(); }
    }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="UnixRootItem"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the root item in the file system path.</param>
    /// <param name="name">The name of the root item.</param>
    private UnixRootItem(FileSystemPathId id, string name) : base(id, name, FileSystemItemType.Root)
    {
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="UnixRootItem"/> class.
    /// </summary>
    /// <param name="status">The status of the file system root item.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="UnixRootItem"/>, or an error message.
    /// </returns>
    public static ErrorOr<UnixRootItem> Create(
        FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        ErrorOr<FileSystemPathId> createPathResult = FileSystemPathId.Create(PATH);
        if (createPathResult.IsError)
            return createPathResult.Errors;
        UnixRootItem newRoot = new UnixRootItem(
            createPathResult.Value,
            PATH);
        ErrorOr<Updated> setStatusResult = newRoot.SetStatus(status);
        if (setStatusResult.IsError)
            return setStatusResult.Errors;
        return newRoot;
    }
    #endregion
}