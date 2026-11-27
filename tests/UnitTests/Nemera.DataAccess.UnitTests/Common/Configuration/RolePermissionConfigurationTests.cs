#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
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
/// Contains unit tests for the <see cref="RolePermissionConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionConfigurationTests"/> class.
    /// </summary>
    public RolePermissionConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndCompositeKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(RolePermissionEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("RolePermissions", tableName);
        Assert.Equal([nameof(RolePermissionEntity.RoleId), nameof(RolePermissionEntity.PermissionId)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldIgnoreTheIdProperty()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(RolePermissionEntity))!;

        // Act
        IProperty? idProperty = entityType.FindProperty(nameof(RolePermissionEntity.Id));

        // Assert
        Assert.Null(idProperty);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipsWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(RolePermissionEntity))!;

        // Act
        List<IForeignKey> foreignKeys = entityType.GetForeignKeys().ToList();

        // Assert
        Assert.Equal(2, foreignKeys.Count);
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(RoleEntity) && foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(PermissionEntity) && foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueIndexOnRoleIdAndPermissionId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(RolePermissionEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes(), candidate => candidate.Properties.Select(property => property.Name).SequenceEqual([nameof(RolePermissionEntity.RoleId), nameof(RolePermissionEntity.PermissionId)]));

        // Assert
        Assert.True(index.IsUnique);
    }
}
