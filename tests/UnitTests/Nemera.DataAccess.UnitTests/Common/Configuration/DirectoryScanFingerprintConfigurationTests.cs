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
/// Contains unit tests for the <see cref="DirectoryScanFingerprintConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryScanFingerprintConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryScanFingerprintConfigurationTests"/> class.
    /// </summary>
    public DirectoryScanFingerprintConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndCompositeKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(DirectoryScanFingerprintEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("DirectoryScanFingerprints", tableName);
        Assert.Equal([nameof(DirectoryScanFingerprintEntity.LibraryId), nameof(DirectoryScanFingerprintEntity.Path)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(DirectoryScanFingerprintEntity))!;

        // Act
        IProperty pathProperty = entityType.GetProperty(nameof(DirectoryScanFingerprintEntity.Path));
        IProperty lastWriteTimeUtcProperty = entityType.GetProperty(nameof(DirectoryScanFingerprintEntity.LastWriteTimeUtc));

        // Assert
        Assert.False(pathProperty.IsNullable);
        Assert.Equal(1024, pathProperty.GetMaxLength());
        Assert.False(lastWriteTimeUtcProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(DirectoryScanFingerprintEntity))!;

        // Act
        IForeignKey foreignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(LibraryEntity), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        Assert.Equal(nameof(DirectoryScanFingerprintEntity.LibraryId), foreignKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexOnLibraryId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(DirectoryScanFingerprintEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes());

        // Assert
        Assert.Equal(nameof(DirectoryScanFingerprintEntity.LibraryId), index.Properties[0].Name);
    }
}
