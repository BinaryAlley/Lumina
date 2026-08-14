#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
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
        Result<UnixRootItem> unixRootItemResult = UnixRootItem.Create(
            _faker.PickRandom<FileSystemItemStatus>()
        );
        if (unixRootItemResult.IsFailure)
            throw new InvalidOperationException("Failed to create File: " + string.Join(", ", unixRootItemResult.Errors));
        return unixRootItemResult.Value;
    }
}
