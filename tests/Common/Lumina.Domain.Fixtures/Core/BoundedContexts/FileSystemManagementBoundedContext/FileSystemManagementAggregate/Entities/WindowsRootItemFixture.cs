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
/// Fixture class for the <see cref="WindowsRootItem"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsRootItemFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="WindowsRootItem"/>.
    /// </summary>
    /// <param name="path">Optional. The root item path.</param>
    /// <param name="name">Optional. The root item name.</param>
    /// <param name="status">Optional. The root item status.</param>
    /// <returns>The created <see cref="WindowsRootItem"/>.</returns>
    public WindowsRootItem Create(
        string? path = null,
        string? name = null,
        FileSystemItemStatus? status = null)
    {
        Result<WindowsRootItem> windowsRootItemResult = WindowsRootItem.Create(
            path ?? _faker.System.FilePath(),
            name ?? _faker.System.FileName(),
            status ?? _faker.PickRandom<FileSystemItemStatus>()
        );
        if (windowsRootItemResult.IsFailure)
            throw new InvalidOperationException("Failed to create WindowsRootItem: " + string.Join(", ", windowsRootItemResult.Errors));
        return windowsRootItemResult.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="WindowsRootItem"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<WindowsRootItem> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
