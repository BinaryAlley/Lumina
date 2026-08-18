#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorIdTests
{
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        MediaContributorId contributorId = MediaContributorId.CreateUnique();

        // Assert
        Assert.NotNull(contributorId);
        Assert.NotEqual(default, contributorId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        MediaContributorId contributorId = MediaContributorId.Create(value);

        // Assert
        Assert.Equal(value, contributorId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaContributorId firstId = _mediaContributorIdFixture.Create(value);
        MediaContributorId secondId = _mediaContributorIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorId firstId = _mediaContributorIdFixture.Create();
        MediaContributorId secondId = _mediaContributorIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
