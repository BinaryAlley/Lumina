#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Models.Core;

/// <summary>
/// Contains unit tests for the <see cref="Entity{TId}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EntityTests
{
    [Fact]
    public void Id_WhenConstructed_ShouldBeSet()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        TestEntity entity = new(id);

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TestEntity firstEntity = new(id);
        TestEntity secondEntity = new(id);

        // Act
        bool result = firstEntity.Equals(secondEntity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        TestEntity firstEntity = new(Guid.NewGuid());
        TestEntity secondEntity = new(Guid.NewGuid());

        // Act
        bool result = firstEntity.Equals(secondEntity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualityOperator_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TestEntity firstEntity = new(id);
        TestEntity secondEntity = new(id);

        // Act
        bool result = firstEntity == secondEntity;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_WithDifferentId_ShouldReturnTrue()
    {
        // Arrange
        TestEntity firstEntity = new(Guid.NewGuid());
        TestEntity secondEntity = new(Guid.NewGuid());

        // Act
        bool result = firstEntity != secondEntity;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TestEntity firstEntity = new(id);
        TestEntity secondEntity = new(id);

        // Act
        int firstHashCode = firstEntity.GetHashCode();
        int secondHashCode = secondEntity.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void CreatedOnUtc_And_UpdatedOnUtc_WhenSet_ShouldBeStored()
    {
        // Arrange
        TestEntity entity = new(Guid.NewGuid());
        DateTime createdOnUtc = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        // Act
        entity.CreatedOnUtc = createdOnUtc;
        entity.UpdatedOnUtc = updatedOnUtc;

        // Assert
        Assert.Equal(createdOnUtc, entity.CreatedOnUtc);
        Assert.Equal(updatedOnUtc, entity.UpdatedOnUtc);
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="Entity{TId}"/> class.
    /// </summary>
    private sealed class TestEntity : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEntity"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public TestEntity(Guid id) : base(id)
        {
        }
    }
}
