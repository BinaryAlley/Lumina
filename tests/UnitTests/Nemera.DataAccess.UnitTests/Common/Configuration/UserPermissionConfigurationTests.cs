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
/// Contains unit tests for the <see cref="UserPermissionConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserPermissionConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPermissionConfigurationTests"/> class.
    /// </summary>
    public UserPermissionConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndCompositeKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserPermissionEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("UserPermissions", tableName);
        Assert.Equal([nameof(UserPermissionEntity.UserId), nameof(UserPermissionEntity.PermissionId)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldIgnoreTheIdProperty()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserPermissionEntity))!;

        // Act
        IProperty? idProperty = entityType.FindProperty(nameof(UserPermissionEntity.Id));

        // Assert
        Assert.Null(idProperty);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipsWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserPermissionEntity))!;

        // Act
        List<IForeignKey> foreignKeys = [.. entityType.GetForeignKeys()];

        // Assert
        Assert.Equal(2, foreignKeys.Count);
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(UserEntity) && foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(PermissionEntity) && foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueIndexOnUserIdAndPermissionId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserPermissionEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes(), candidate => candidate.Properties.Select(property => property.Name).SequenceEqual([nameof(UserPermissionEntity.UserId), nameof(UserPermissionEntity.PermissionId)]));

        // Assert
        Assert.True(index.IsUnique);
    }
}
