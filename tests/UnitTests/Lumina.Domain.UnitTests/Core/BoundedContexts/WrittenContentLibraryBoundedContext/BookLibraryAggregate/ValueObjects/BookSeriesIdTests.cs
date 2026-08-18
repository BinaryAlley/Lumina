#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="BookSeriesId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookSeriesIdTests
{
    private readonly BookSeriesIdFixture _bookSeriesIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        BookSeriesId seriesId = BookSeriesId.CreateUnique();

        // Assert
        Assert.NotNull(seriesId);
        Assert.NotEqual(default, seriesId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        BookSeriesId seriesId = BookSeriesId.Create(value);

        // Assert
        Assert.Equal(value, seriesId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        BookSeriesId firstId = _bookSeriesIdFixture.Create(value);
        BookSeriesId secondId = _bookSeriesIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        BookSeriesId firstId = _bookSeriesIdFixture.Create();
        BookSeriesId secondId = _bookSeriesIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
