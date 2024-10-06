#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Entities.Fixtures;

/// <summary>
/// Fixture class for the <see cref="UnixRootItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixRootItemFixture
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid <see cref="UnixRootItem"/>.
    /// </summary>
    /// <param name="status">The status of the UNIX root item.</param>
    /// <returns>The created <see cref="UnixRootItem"/>.</returns>
    public UnixRootItem CreateUnixRootItem(FileSystemItemStatus status = FileSystemItemStatus.Accessible)
    {
        ErrorOr<UnixRootItem> unixRootItemResult = UnixRootItem.Create(status);

        if (unixRootItemResult.IsError)
            throw new InvalidOperationException("Failed to create UnixRootItem: " + string.Join(", ", unixRootItemResult.Errors));
        return unixRootItemResult.Value;
    }
    #endregion
}
