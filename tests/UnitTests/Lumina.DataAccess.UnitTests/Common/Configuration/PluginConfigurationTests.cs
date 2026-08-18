#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="PluginConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfigurationTests"/> class.
    /// </summary>
    public PluginConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PluginEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Plugins", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PluginEntity))!;

        // Act
        IProperty nameProperty = entityType.GetProperty(nameof(PluginEntity.Name));
        IProperty authorProperty = entityType.GetProperty(nameof(PluginEntity.Author));
        IProperty versionProperty = entityType.GetProperty(nameof(PluginEntity.Version));
        IProperty loadStatusProperty = entityType.GetProperty(nameof(PluginEntity.LoadStatus));

        // Assert
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(255, nameProperty.GetMaxLength());
        Assert.False(authorProperty.IsNullable);
        Assert.Equal(255, authorProperty.GetMaxLength());
        Assert.False(versionProperty.IsNullable);
        Assert.Equal(50, versionProperty.GetMaxLength());
        Assert.Equal(typeof(PluginLoadStatus), loadStatusProperty.ClrType);
        Assert.Equal("TEXT", loadStatusProperty.GetColumnType());
        Assert.Equal(20, loadStatusProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PluginEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(PluginEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(PluginEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
