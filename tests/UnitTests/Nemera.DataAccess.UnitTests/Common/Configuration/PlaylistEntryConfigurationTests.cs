#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistEntryConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistEntryConfigurationTests"/> class.
    /// </summary>
    public PlaylistEntryConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    /// <summary>
    /// Gets the design-time model, which retains configuration such as column order that the read-optimized runtime model omits.
    /// </summary>
    private IModel DesignTimeModel => _context.GetService<IDesignTimeModel>().Model;

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("PlaylistEntries", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValueAndOrderItFirst()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
        Assert.Equal(0, idProperty.GetColumnOrder());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldStoreTheMediaItemTypeAsABoundedRequiredString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IProperty mediaItemTypeProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.MediaItemType));

        // Assert
        Assert.Equal(typeof(PlaylistMediaItemType), mediaItemTypeProperty.ClrType);
        Assert.Equal(typeof(string), mediaItemTypeProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", mediaItemTypeProperty.GetColumnType());
        Assert.Equal(20, mediaItemTypeProperty.GetMaxLength());
        Assert.False(mediaItemTypeProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheRequiredColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IProperty playlistIdProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.PlaylistId));
        IProperty mediaItemIdProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.MediaItemId));
        IProperty positionProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.Position));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.CreatedBy));

        // Assert
        Assert.False(playlistIdProperty.IsNullable);
        Assert.False(mediaItemIdProperty.IsNullable);
        Assert.False(positionProperty.IsNullable);
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheNullableAuditColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.UpdatedBy));

        // Assert
        Assert.True(updatedOnUtcProperty.IsNullable);
        Assert.True(updatedByProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
        Assert.Null(updatedByProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheUniquePositionIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IIndex positionIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(PlaylistEntryEntity.PlaylistId),
                nameof(PlaylistEntryEntity.Position)
            ]));

        // Assert
        Assert.True(positionIndex.IsUnique);
        Assert.Equal(2, positionIndex.Properties.Count);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheMediaItemLookupIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IIndex mediaItemIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(PlaylistEntryEntity.MediaItemId)]));

        // Assert
        Assert.False(mediaItemIndex.IsUnique);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldOwnTheEntryThroughThePlaylistWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IForeignKey playlistForeignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(PlaylistEntity), playlistForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(PlaylistEntryEntity.PlaylistId), playlistForeignKey.Properties[0].Name);
        Assert.Equal(DeleteBehavior.Cascade, playlistForeignKey.DeleteBehavior);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotReferenceTheMediaItemByAForeignKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        List<IForeignKey> foreignKeys = [.. entityType.GetForeignKeys()];

        // Assert
        // The media item is referenced by Id only, so an entry survives the deletion of the item it references.
        Assert.DoesNotContain(foreignKeys, foreignKey => foreignKey.Properties.Any(property => property.Name == nameof(PlaylistEntryEntity.MediaItemId)));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldOrderEveryColumnAsConfigured()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.Id));
        IProperty playlistIdProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.PlaylistId));
        IProperty mediaItemIdProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.MediaItemId));
        IProperty mediaItemTypeProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.MediaItemType));
        IProperty positionProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.Position));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(PlaylistEntryEntity.UpdatedBy));

        // Assert
        Assert.Equal(0, idProperty.GetColumnOrder());
        Assert.Equal(1, playlistIdProperty.GetColumnOrder());
        Assert.Equal(2, mediaItemIdProperty.GetColumnOrder());
        Assert.Equal(3, mediaItemTypeProperty.GetColumnOrder());
        Assert.Equal(4, positionProperty.GetColumnOrder());
        Assert.Equal(5, createdOnUtcProperty.GetColumnOrder());
        Assert.Equal(6, createdByProperty.GetColumnOrder());
        Assert.Equal(7, updatedOnUtcProperty.GetColumnOrder());
        Assert.Equal(8, updatedByProperty.GetColumnOrder());
    }
}
