#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;

/// <summary>
/// Fixture class for the <see cref="UnixRootItem"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixRootItemFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UnixRootItem"/>.
    /// </summary>
    /// <param name="status">Optional. The root item status.</param>
    /// <returns>The created <see cref="UnixRootItem"/>.</returns>
    public UnixRootItem Create(FileSystemItemStatus? status = null)
    {
        Result<UnixRootItem> unixRootItemResult = UnixRootItem.Create(
            status ?? _faker.PickRandom<FileSystemItemStatus>()
        );
        if (unixRootItemResult.IsFailure)
            throw new InvalidOperationException("Failed to create UnixRootItem: " + string.Join(", ", unixRootItemResult.Errors));
        return unixRootItemResult.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="UnixRootItem"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UnixRootItem> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
