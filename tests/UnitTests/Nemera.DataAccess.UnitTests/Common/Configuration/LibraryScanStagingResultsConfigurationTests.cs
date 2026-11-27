#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
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
/// Contains unit tests for the <see cref="LibraryScanStagingResultsConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanStagingResultsConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanStagingResultsConfigurationTests"/> class.
    /// </summary>
    public LibraryScanStagingResultsConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndCompositeKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanStagingResultsEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("LibraryScanStagingResults", tableName);
        Assert.Equal([nameof(LibraryScanStagingResultsEntity.LibraryScanId), nameof(LibraryScanStagingResultsEntity.Path)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanStagingResultsEntity))!;

        // Act
        IProperty pathProperty = entityType.GetProperty(nameof(LibraryScanStagingResultsEntity.Path));
        IProperty needsRehashProperty = entityType.GetProperty(nameof(LibraryScanStagingResultsEntity.NeedsRehash));
        IProperty isNewProperty = entityType.GetProperty(nameof(LibraryScanStagingResultsEntity.IsNew));

        // Assert
        Assert.False(pathProperty.IsNullable);
        Assert.Equal(1024, pathProperty.GetMaxLength());
        Assert.False(needsRehashProperty.IsNullable);
        Assert.False(isNewProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanStagingResultsEntity))!;

        // Act
        IForeignKey foreignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(LibraryScanEntity), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        Assert.Equal(nameof(LibraryScanStagingResultsEntity.LibraryScanId), foreignKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexOnLibraryScanIdAndNeedsRehash()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanStagingResultsEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes());

        // Assert
        Assert.Equal([nameof(LibraryScanStagingResultsEntity.LibraryScanId), nameof(LibraryScanStagingResultsEntity.NeedsRehash)], index.Properties.Select(property => property.Name));
    }
}
