#region ========================================================================= USING =====================================================================================
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
/// Contains unit tests for the <see cref="UserConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigurationTests"/> class.
    /// </summary>
    public UserConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Users", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserEntity))!;

        // Act
        IProperty usernameProperty = entityType.GetProperty(nameof(UserEntity.Username));
        IProperty passwordProperty = entityType.GetProperty(nameof(UserEntity.Password));
        IProperty tempPasswordProperty = entityType.GetProperty(nameof(UserEntity.TempPassword));
        IProperty totpSecretProperty = entityType.GetProperty(nameof(UserEntity.TotpSecret));

        // Assert
        Assert.False(usernameProperty.IsNullable);
        Assert.Equal(255, usernameProperty.GetMaxLength());
        Assert.False(passwordProperty.IsNullable);
        Assert.Null(tempPasswordProperty.GetDefaultValue());
        Assert.Null(totpSecretProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipsWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserEntity))!;

        // Act
        List<INavigation> navigations = entityType.GetNavigations().ToList();
        List<IForeignKey> referencingForeignKeys = entityType.GetReferencingForeignKeys().ToList();

        // Assert
        Assert.Contains(navigations, navigation => navigation.Name == nameof(UserEntity.Libraries) && navigation.IsCollection);
        Assert.Contains(navigations, navigation => navigation.Name == nameof(UserEntity.UserRole) && !navigation.IsCollection);
        Assert.Contains(navigations, navigation => navigation.Name == nameof(UserEntity.UserPermissions) && navigation.IsCollection);
        Assert.All(referencingForeignKeys, foreignKey => Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueIndexOnUsername()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserEntity))!;

        // Act
        IIndex? usernameIndex = entityType.GetIndexes().SingleOrDefault(index => index.Properties.Any(property => property.Name == nameof(UserEntity.Username)));

        // Assert
        Assert.NotNull(usernameIndex);
        Assert.True(usernameIndex!.IsUnique);
        Assert.Equal(nameof(UserEntity.Username), usernameIndex.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(UserEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(UserEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(UserEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(UserEntity.UpdatedBy));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
        Assert.Null(updatedByProperty.GetDefaultValue());
    }
}
