#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="UserRoleConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRoleConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleConfigurationTests"/> class.
    /// </summary>
    public UserRoleConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserRoleEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("UserRoles", tableName);
        Assert.Equal([nameof(UserRoleEntity.UserId)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldIgnoreTheIdProperty()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserRoleEntity))!;

        // Act
        IProperty? idProperty = entityType.FindProperty(nameof(UserRoleEntity.Id));

        // Assert
        Assert.Null(idProperty);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipsWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserRoleEntity))!;

        // Act
        List<IForeignKey> foreignKeys = [.. entityType.GetForeignKeys()];

        // Assert
        Assert.Equal(2, foreignKeys.Count);
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(UserEntity) && foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(RoleEntity) && foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueIndexOnUserIdAndRoleId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserRoleEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes(), candidate => candidate.Properties.Select(property => property.Name).SequenceEqual([nameof(UserRoleEntity.UserId), nameof(UserRoleEntity.RoleId)]));

        // Assert
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserRoleEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(UserRoleEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(UserRoleEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
