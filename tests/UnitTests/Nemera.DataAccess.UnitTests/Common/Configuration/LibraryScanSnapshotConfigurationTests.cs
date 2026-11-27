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
/// Contains unit tests for the <see cref="LibraryScanSnapshotConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanSnapshotConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanSnapshotConfigurationTests"/> class.
    /// </summary>
    public LibraryScanSnapshotConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndCompositeKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanSnapshotEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("LibraryScanSnapshots", tableName);
        Assert.Equal([nameof(LibraryScanSnapshotEntity.LibraryId), nameof(LibraryScanSnapshotEntity.Path)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanSnapshotEntity))!;

        // Act
        IProperty pathProperty = entityType.GetProperty(nameof(LibraryScanSnapshotEntity.Path));
        IProperty contentHashProperty = entityType.GetProperty(nameof(LibraryScanSnapshotEntity.ContentHash));

        // Assert
        Assert.False(pathProperty.IsNullable);
        Assert.Equal(1024, pathProperty.GetMaxLength());
        Assert.False(contentHashProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanSnapshotEntity))!;

        // Act
        IForeignKey foreignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(LibraryEntity), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        Assert.Equal(nameof(LibraryScanSnapshotEntity.LibraryId), foreignKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexOnLibraryId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanSnapshotEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes());

        // Assert
        Assert.Equal(nameof(LibraryScanSnapshotEntity.LibraryId), index.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanSnapshotEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(LibraryScanSnapshotEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(LibraryScanSnapshotEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
