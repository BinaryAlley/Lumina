#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanConfigurationTests"/> class.
    /// </summary>
    public LibraryScanConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("LibraryScans", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureStatusAsRequiredString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanEntity))!;

        // Act
        IProperty statusProperty = entityType.GetProperty(nameof(LibraryScanEntity.Status));

        // Assert
        Assert.False(statusProperty.IsNullable);
        Assert.Equal(typeof(LibraryScanJobStatus), statusProperty.ClrType);
        Assert.Equal("TEXT", statusProperty.GetColumnType());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationshipsWithUserAndLibrary()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanEntity))!;

        // Act
        List<IForeignKey> foreignKeys = [.. entityType.GetForeignKeys()];

        // Assert
        Assert.Equal(2, foreignKeys.Count);
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(LibraryEntity) && foreignKey.Properties[0].Name == nameof(LibraryScanEntity.LibraryId));
        Assert.Contains(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(UserEntity) && foreignKey.Properties[0].Name == nameof(LibraryScanEntity.UserId));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureDescendingIndexOnLibraryIdStatusAndCreatedOnUtc()
    {
        // Arrange
        // the read-optimized runtime model does not store the index sort order, so the design-time model is inspected instead
        IDesignTimeModel designTimeModel = _context.GetService<IDesignTimeModel>();
        IEntityType entityType = designTimeModel.Model.FindEntityType(typeof(LibraryScanEntity))!;

        // Act
        IIndex index = Assert.Single(entityType.GetIndexes(), candidate => candidate.Properties.Select(property => property.Name).SequenceEqual([nameof(LibraryScanEntity.LibraryId), nameof(LibraryScanEntity.Status), nameof(LibraryScanEntity.CreatedOnUtc)]));

        // Assert
        Assert.Equal([true, false, true], index.IsDescending);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryScanEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(LibraryScanEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(LibraryScanEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
