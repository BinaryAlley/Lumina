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
        Result<WindowsRootItem> windowsRootItemResult = WindowsRootItem.Create(
            _faker.System.FilePath(),
            _faker.System.FileName(),
            _faker.PickRandom<FileSystemItemStatus>()
        );
        if (windowsRootItemResult.IsFailure)
            throw new InvalidOperationException("Failed to create File: " + string.Join(", ", windowsRootItemResult.Errors));
        return windowsRootItemResult.Value;
    }
}
