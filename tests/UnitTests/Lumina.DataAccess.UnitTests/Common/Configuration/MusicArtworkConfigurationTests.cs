#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
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
/// Contains unit tests for the <see cref="MusicArtworkConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtworkConfigurationTests : IDisposable
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkConfigurationTests"/> class.
    /// </summary>
    public MusicArtworkConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicArtworkEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("MusicArtwork", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValue()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicArtworkEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(MusicArtworkEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicArtworkEntity))!;

        // Act
        IProperty ownerTypeProperty = entityType.GetProperty(nameof(MusicArtworkEntity.OwnerType));
        IProperty ownerIdProperty = entityType.GetProperty(nameof(MusicArtworkEntity.OwnerId));
        IProperty artworkTypeProperty = entityType.GetProperty(nameof(MusicArtworkEntity.ArtworkType));
        IProperty ordinalProperty = entityType.GetProperty(nameof(MusicArtworkEntity.Ordinal));
        IProperty fileNameProperty = entityType.GetProperty(nameof(MusicArtworkEntity.FileName));
        IProperty contentHashProperty = entityType.GetProperty(nameof(MusicArtworkEntity.ContentHash));
        IProperty statusProperty = entityType.GetProperty(nameof(MusicArtworkEntity.Status));
        IProperty providerProperty = entityType.GetProperty(nameof(MusicArtworkEntity.Provider));
        IProperty lastUpdateUtcProperty = entityType.GetProperty(nameof(MusicArtworkEntity.LastUpdateUtc));

        // Assert
        Assert.False(ownerTypeProperty.IsNullable);
        Assert.Equal(typeof(MusicArtworkOwnerType), ownerTypeProperty.ClrType);
        Assert.Equal("TEXT", ownerTypeProperty.GetColumnType());
        Assert.Equal(50, ownerTypeProperty.GetMaxLength());
        Assert.False(ownerIdProperty.IsNullable);
        Assert.False(artworkTypeProperty.IsNullable);
        Assert.Equal(typeof(ArtworkType), artworkTypeProperty.ClrType);
        Assert.Equal("TEXT", artworkTypeProperty.GetColumnType());
        Assert.Equal(50, artworkTypeProperty.GetMaxLength());
        Assert.False(ordinalProperty.IsNullable);
        Assert.True(fileNameProperty.IsNullable);
        Assert.Equal(2048, fileNameProperty.GetMaxLength());
        Assert.False(contentHashProperty.IsNullable);
        Assert.False(statusProperty.IsNullable);
        Assert.Equal(typeof(ArtworkStatus), statusProperty.ClrType);
        Assert.Equal("TEXT", statusProperty.GetColumnType());
        Assert.Equal(20, statusProperty.GetMaxLength());
        Assert.True(providerProperty.IsNullable);
        Assert.Equal(100, providerProperty.GetMaxLength());
        Assert.True(lastUpdateUtcProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueOwnerArtworkIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicArtworkEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        IIndex uniqueIndex = Assert.Single(indexes, index => index.IsUnique);
        Assert.Equal(
            [nameof(MusicArtworkEntity.OwnerType), nameof(MusicArtworkEntity.OwnerId), nameof(MusicArtworkEntity.ArtworkType), nameof(MusicArtworkEntity.Ordinal)],
            uniqueIndex.Properties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexOnStatus()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicArtworkEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(MusicArtworkEntity.Status)]));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicArtworkEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(MusicArtworkEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(MusicArtworkEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(MusicArtworkEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(MusicArtworkEntity.UpdatedBy));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
        Assert.True(updatedOnUtcProperty.IsNullable);
        Assert.True(updatedByProperty.IsNullable);
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
    }
}
