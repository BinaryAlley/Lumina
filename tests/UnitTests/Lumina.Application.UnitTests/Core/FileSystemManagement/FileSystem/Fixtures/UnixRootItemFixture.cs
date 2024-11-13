#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.FileSystem.Fixtures;

/// <summary>
/// Fixture class for the <see cref="UnixRootItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixRootItemFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UnixRootItem"/>.
    /// </summary>
    /// <returns>The created <see cref="UnixRootItem"/>.</returns>
    public UnixRootItem Create()
    {
        ErrorOr<UnixRootItem> unixRootItemResult = UnixRootItem.Create(
            _faker.PickRandom<FileSystemItemStatus>()
        );
        if (unixRootItemResult.IsError)
            throw new InvalidOperationException("Failed to create File: " + string.Join(", ", unixRootItemResult.Errors));
        return unixRootItemResult.Value;
    }
}
