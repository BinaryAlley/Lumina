#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="BookLibraryId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLibraryIdTests
{
    private readonly BookLibraryIdFixture _bookLibraryIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        Result<BookLibraryId> result = BookLibraryId.CreateUnique();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(default, result.Value.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        Result<BookLibraryId> result = BookLibraryId.Create(value);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(value, result.Value.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        BookLibraryId firstId = _bookLibraryIdFixture.Create(value);
        BookLibraryId secondId = _bookLibraryIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        BookLibraryId firstId = _bookLibraryIdFixture.Create();
        BookLibraryId secondId = _bookLibraryIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
