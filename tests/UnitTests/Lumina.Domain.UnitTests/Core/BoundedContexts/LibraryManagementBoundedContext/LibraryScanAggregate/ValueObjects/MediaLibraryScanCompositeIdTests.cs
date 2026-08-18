#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanCompositeId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanCompositeIdTests
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithScanIdAndUserId_ShouldCreateCompositeIdWithThoseValues()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();

        // Act
        MediaLibraryScanCompositeId compositeId = MediaLibraryScanCompositeId.Create(scanId, userId);

        // Assert
        Assert.Equal(scanId, compositeId.ScanId);
        Assert.Equal(userId, compositeId.UserId);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();

        // Act
        MediaLibraryScanCompositeId firstId = _mediaLibraryScanCompositeIdFixture.Create(scanId: scanId, userId: userId);
        MediaLibraryScanCompositeId secondId = _mediaLibraryScanCompositeIdFixture.Create(scanId: scanId, userId: userId);

        // Assert
        Assert.Equal(firstId, secondId);
    }

    [Fact]
    public void Equals_WithDifferentScanId_ShouldReturnFalse()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();

        // Act
        MediaLibraryScanCompositeId firstId = _mediaLibraryScanCompositeIdFixture.Create(userId: userId);
        MediaLibraryScanCompositeId secondId = _mediaLibraryScanCompositeIdFixture.Create(userId: userId);

        // Assert
        Assert.NotEqual(firstId, secondId);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();

        // Act
        MediaLibraryScanCompositeId firstId = _mediaLibraryScanCompositeIdFixture.Create(scanId: scanId, userId: userId);
        MediaLibraryScanCompositeId secondId = _mediaLibraryScanCompositeIdFixture.Create(scanId: scanId, userId: userId);

        // Assert
        Assert.Equal(firstId.GetHashCode(), secondId.GetHashCode());
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnFormattedString()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create(scanId: scanId, userId: userId);

        // Act
        string result = compositeId.ToString();

        // Assert
        Assert.Equal($"{scanId}-{userId}", result);
    }
}
