#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
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
/// Contains unit tests for the <see cref="LibraryMetadataProviderConfigurationConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMetadataProviderConfigurationConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMetadataProviderConfigurationConfigurationTests"/> class.
    /// </summary>
    public LibraryMetadataProviderConfigurationConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryMetadataProviderConfigurationEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("LibraryMetadataProviderConfigurations", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarPropertiesAsRequired()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryMetadataProviderConfigurationEntity))!;

        // Act
        IProperty libraryIdProperty = entityType.GetProperty(nameof(LibraryMetadataProviderConfigurationEntity.LibraryId));
        IProperty pluginIdProperty = entityType.GetProperty(nameof(LibraryMetadataProviderConfigurationEntity.PluginId));
        IProperty isEnabledProperty = entityType.GetProperty(nameof(LibraryMetadataProviderConfigurationEntity.IsEnabled));
        IProperty rankProperty = entityType.GetProperty(nameof(LibraryMetadataProviderConfigurationEntity.Rank));

        // Assert
        Assert.False(libraryIdProperty.IsNullable);
        Assert.False(pluginIdProperty.IsNullable);
        Assert.False(isEnabledProperty.IsNullable);
        Assert.False(rankProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexOnLibraryIdAndPluginId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryMetadataProviderConfigurationEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes());

        // Assert
        Assert.Equal([nameof(LibraryMetadataProviderConfigurationEntity.LibraryId), nameof(LibraryMetadataProviderConfigurationEntity.PluginId)], index.Properties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryMetadataProviderConfigurationEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(LibraryMetadataProviderConfigurationEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(LibraryMetadataProviderConfigurationEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
