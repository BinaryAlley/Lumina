#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
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
/// Contains unit tests for the <see cref="FavoriteConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoriteConfigurationTests"/> class.
    /// </summary>
    public FavoriteConfigurationTests()
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
        IEntityType entityType = _context.Model.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Favorites", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValueAndOrderItFirst()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(FavoriteEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
        Assert.Equal(0, idProperty.GetColumnOrder());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureCascadingRelationshipWithUser()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IForeignKey userForeignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(UserEntity), userForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(FavoriteEntity.UserId), userForeignKey.Properties[0].Name);
        Assert.Equal(DeleteBehavior.Cascade, userForeignKey.DeleteBehavior);
        Assert.True(userForeignKey.IsRequired);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldStoreTheMediaKindAsABoundedRequiredString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IProperty mediaKindProperty = entityType.GetProperty(nameof(FavoriteEntity.MediaKind));

        // Assert
        Assert.Equal(typeof(FavoriteMediaKind), mediaKindProperty.ClrType);
        Assert.Equal(typeof(string), mediaKindProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", mediaKindProperty.GetColumnType());
        Assert.Equal(20, mediaKindProperty.GetMaxLength());
        Assert.False(mediaKindProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheRequiredColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IProperty userIdProperty = entityType.GetProperty(nameof(FavoriteEntity.UserId));
        IProperty mediaItemIdProperty = entityType.GetProperty(nameof(FavoriteEntity.MediaItemId));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(FavoriteEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(FavoriteEntity.CreatedBy));

        // Assert
        Assert.False(userIdProperty.IsNullable);
        Assert.False(mediaItemIdProperty.IsNullable);
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheNullableAuditColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(FavoriteEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(FavoriteEntity.UpdatedBy));

        // Assert
        Assert.True(updatedOnUtcProperty.IsNullable);
        Assert.True(updatedByProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
        Assert.Null(updatedByProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheUniqueBusinessKeyIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IIndex businessKeyIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(FavoriteEntity.UserId),
                nameof(FavoriteEntity.MediaKind),
                nameof(FavoriteEntity.MediaItemId)
            ]));

        // Assert
        Assert.True(businessKeyIndex.IsUnique);
        Assert.Equal(3, businessKeyIndex.Properties.Count);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheListingIndexes()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IIndex everyKindListingIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(FavoriteEntity.UserId),
                nameof(FavoriteEntity.CreatedOnUtc)
            ]));
        IIndex oneKindListingIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(FavoriteEntity.UserId),
                nameof(FavoriteEntity.MediaKind),
                nameof(FavoriteEntity.CreatedOnUtc)
            ]));

        // Assert
        Assert.False(everyKindListingIndex.IsUnique);
        Assert.False(oneKindListingIndex.IsUnique);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldOrderEveryColumnAsConfigured()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(FavoriteEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(FavoriteEntity.Id));
        IProperty userIdProperty = entityType.GetProperty(nameof(FavoriteEntity.UserId));
        IProperty mediaKindProperty = entityType.GetProperty(nameof(FavoriteEntity.MediaKind));
        IProperty mediaItemIdProperty = entityType.GetProperty(nameof(FavoriteEntity.MediaItemId));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(FavoriteEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(FavoriteEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(FavoriteEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(FavoriteEntity.UpdatedBy));

        // Assert
        Assert.Equal(0, idProperty.GetColumnOrder());
        Assert.Equal(1, userIdProperty.GetColumnOrder());
        Assert.Equal(2, mediaKindProperty.GetColumnOrder());
        Assert.Equal(3, mediaItemIdProperty.GetColumnOrder());
        Assert.Equal(4, createdOnUtcProperty.GetColumnOrder());
        Assert.Equal(5, createdByProperty.GetColumnOrder());
        Assert.Equal(6, updatedOnUtcProperty.GetColumnOrder());
        Assert.Equal(7, updatedByProperty.GetColumnOrder());
    }
}
