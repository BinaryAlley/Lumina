#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="LibraryId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryIdTests
{
    private readonly LibraryIdFixture _libraryIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        LibraryId libraryId = LibraryId.CreateUnique();

        // Assert
        Assert.NotNull(libraryId);
        Assert.NotEqual(default, libraryId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        LibraryId libraryId = LibraryId.Create(value);

        // Assert
        Assert.Equal(value, libraryId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        LibraryId firstId = _libraryIdFixture.Create(value);
        LibraryId secondId = _libraryIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        LibraryId firstId = _libraryIdFixture.Create(value);
        LibraryId secondId = _libraryIdFixture.Create(value);

        // Act
        int firstHashCode = firstId.GetHashCode();
        int secondHashCode = secondId.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        LibraryId firstId = _libraryIdFixture.Create();
        LibraryId secondId = _libraryIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
