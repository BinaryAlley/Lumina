#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="ScanId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanIdTests
{
    private readonly ScanIdFixture _scanIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        ScanId scanId = ScanId.CreateUnique();

        // Assert
        Assert.NotNull(scanId);
        Assert.NotEqual(default, scanId.Value);
    }

    [Fact]
    public void CreateUnique_WhenCalledTwice_ShouldReturnDistinctIds()
    {
        // Act
        ScanId firstId = ScanId.CreateUnique();
        ScanId secondId = ScanId.CreateUnique();

        // Assert
        Assert.NotEqual(firstId.Value, secondId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        ScanId scanId = ScanId.Create(value);

        // Assert
        Assert.Equal(value, scanId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        ScanId firstId = _scanIdFixture.Create(value);
        ScanId secondId = _scanIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        ScanId firstId = _scanIdFixture.Create();
        ScanId secondId = _scanIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
