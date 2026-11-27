#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="PermissionConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionConfigurationTests"/> class.
    /// </summary>
    public PermissionConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PermissionEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Permissions", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigurePermissionNameAsRequiredString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PermissionEntity))!;

        // Act
        IProperty permissionNameProperty = entityType.GetProperty(nameof(PermissionEntity.PermissionName));

        // Assert
        Assert.False(permissionNameProperty.IsNullable);
        Assert.Equal(100, permissionNameProperty.GetMaxLength());
        Assert.Equal(typeof(AuthorizationPermission), permissionNameProperty.ClrType);
        Assert.Equal("TEXT", permissionNameProperty.GetColumnType());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PermissionEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(PermissionEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(PermissionEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
