#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanJobProgressMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobProgressMappingTests
{
    private readonly MediaLibraryScanJobProgressFixture _mediaLibraryScanJobProgressFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidValueObject_ShouldMapCorrectly()
    {
        // Arrange
        MediaLibraryScanJobProgress progress = _mediaLibraryScanJobProgressFixture.Create(
            completedItems: 3,
            totalItems: 10,
            currentOperation: "Hashing files");

        // Act
        MediaLibraryScanJobProgressResponse result = progress.ToResponse();

        // Assert
        Assert.Equal(progress.CompletedItems, result.CompletedItems);
        Assert.Equal(progress.TotalItems, result.TotalItems);
        Assert.Equal(progress.CurrentOperation, result.CurrentOperation);
        Assert.Equal(progress.ProgressPercentage, result.ProgressPercentage);
    }
}
