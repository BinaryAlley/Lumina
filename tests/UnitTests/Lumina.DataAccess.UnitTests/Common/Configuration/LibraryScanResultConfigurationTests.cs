#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanResultConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanResultConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanResultConfigurationTests"/> class.
    /// </summary>
    public LibraryScanResultConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndCompositeKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanResultEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("LibraryScanResults", tableName);
        Assert.Equal([nameof(LibraryScanResultEntity.LibraryScanId), nameof(LibraryScanResultEntity.Path)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanResultEntity))!;

        // Act
        IProperty pathProperty = entityType.GetProperty(nameof(LibraryScanResultEntity.Path));
        IProperty statusProperty = entityType.GetProperty(nameof(LibraryScanResultEntity.Status));
        IProperty fileSizeProperty = entityType.GetProperty(nameof(LibraryScanResultEntity.FileSize));

        // Assert
        Assert.False(pathProperty.IsNullable);
        Assert.Equal(1024, pathProperty.GetMaxLength());
        Assert.Equal(typeof(LibraryScanFileStatus), statusProperty.ClrType);
        Assert.Equal("TEXT", statusProperty.GetColumnType());
        Assert.Equal(10, statusProperty.GetMaxLength());
        Assert.False(fileSizeProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanResultEntity))!;

        // Act
        IForeignKey foreignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(LibraryScanEntity), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        Assert.Equal(nameof(LibraryScanResultEntity.LibraryScanId), foreignKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexes()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanResultEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(LibraryScanResultEntity.ContentHash), nameof(LibraryScanResultEntity.FileSize), nameof(LibraryScanResultEntity.Path)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(LibraryScanResultEntity.Path)]));
    }
}
