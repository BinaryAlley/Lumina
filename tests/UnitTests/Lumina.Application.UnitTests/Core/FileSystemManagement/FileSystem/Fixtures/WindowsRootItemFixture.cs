#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.FileSystem.Fixtures;

/// <summary>
/// Fixture class for the <see cref="WindowsRootItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsRootItemFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="WindowsRootItem"/>.
    /// </summary>
    /// <returns>The created <see cref="WindowsRootItem"/>.</returns>
    public WindowsRootItem Create()
    {
        ErrorOr<WindowsRootItem> windowsRootItemResult = WindowsRootItem.Create(
            _faker.System.FilePath(),
            _faker.System.FileName(),
            _faker.PickRandom<FileSystemItemStatus>()
        );
        if (windowsRootItemResult.IsError)
            throw new InvalidOperationException("Failed to create File: " + string.Join(", ", windowsRootItemResult.Errors));
        return windowsRootItemResult.Value;
    }
}
