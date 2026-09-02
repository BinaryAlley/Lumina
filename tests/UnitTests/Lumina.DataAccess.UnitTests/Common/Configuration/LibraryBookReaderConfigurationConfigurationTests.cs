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
/// Contains unit tests for the <see cref="LibraryBookReaderConfigurationConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryBookReaderConfigurationConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryBookReaderConfigurationConfigurationTests"/> class.
    /// </summary>
    public LibraryBookReaderConfigurationConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryBookReaderConfigurationEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("LibraryBookReaderConfigurations", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarPropertiesAsRequired()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryBookReaderConfigurationEntity))!;

        // Act
        IProperty libraryIdProperty = entityType.GetProperty(nameof(LibraryBookReaderConfigurationEntity.LibraryId));
        IProperty pluginIdProperty = entityType.GetProperty(nameof(LibraryBookReaderConfigurationEntity.PluginId));
        IProperty isEnabledProperty = entityType.GetProperty(nameof(LibraryBookReaderConfigurationEntity.IsEnabled));

        // Assert
        Assert.False(libraryIdProperty.IsNullable);
        Assert.False(pluginIdProperty.IsNullable);
        Assert.False(isEnabledProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexOnLibraryIdAndPluginId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryBookReaderConfigurationEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes());

        // Assert
        Assert.Equal([nameof(LibraryBookReaderConfigurationEntity.LibraryId), nameof(LibraryBookReaderConfigurationEntity.PluginId)], index.Properties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryBookReaderConfigurationEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(LibraryBookReaderConfigurationEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(LibraryBookReaderConfigurationEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
