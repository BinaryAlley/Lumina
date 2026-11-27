#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
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
/// Contains unit tests for the <see cref="PlaylistConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistConfigurationTests"/> class.
    /// </summary>
    public PlaylistConfigurationTests()
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
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Playlists", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValueAndOrderItFirst()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(PlaylistEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
        Assert.Equal(0, idProperty.GetColumnOrder());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureCascadingRelationshipWithUser()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IForeignKey userForeignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(UserEntity), userForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(PlaylistEntity.UserId), userForeignKey.Properties[0].Name);
        Assert.Equal(DeleteBehavior.Cascade, userForeignKey.DeleteBehavior);
        Assert.True(userForeignKey.IsRequired);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldStoreThePlaylistTypeAsABoundedRequiredString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IProperty typeProperty = entityType.GetProperty(nameof(PlaylistEntity.Type));

        // Assert
        Assert.Equal(typeof(PlaylistType), typeProperty.ClrType);
        Assert.Equal(typeof(string), typeProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", typeProperty.GetColumnType());
        Assert.Equal(20, typeProperty.GetMaxLength());
        Assert.False(typeProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheNameAndDescriptionLengths()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IProperty nameProperty = entityType.GetProperty(nameof(PlaylistEntity.Name));
        IProperty descriptionProperty = entityType.GetProperty(nameof(PlaylistEntity.Description));

        // Assert
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(255, nameProperty.GetMaxLength());
        Assert.True(descriptionProperty.IsNullable);
        Assert.Equal(2000, descriptionProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheRequiredColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IProperty userIdProperty = entityType.GetProperty(nameof(PlaylistEntity.UserId));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(PlaylistEntity.CreatedBy));
        IProperty entryCountProperty = entityType.GetProperty(nameof(PlaylistEntity.EntryCount));

        // Assert
        Assert.False(userIdProperty.IsNullable);
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
        Assert.False(entryCountProperty.IsNullable);
        Assert.Equal(0, entryCountProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheNullableAuditColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(PlaylistEntity.UpdatedBy));

        // Assert
        Assert.True(updatedOnUtcProperty.IsNullable);
        Assert.True(updatedByProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
        Assert.Null(updatedByProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheVersionAsAConcurrencyTokenStartingAtZero()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IProperty versionProperty = entityType.GetProperty(nameof(PlaylistEntity.Version));

        // Assert
        Assert.True(versionProperty.IsConcurrencyToken);
        Assert.Equal(0L, versionProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheEntriesRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;
        IEntityType entryEntityType = _context.Model.FindEntityType(typeof(PlaylistEntryEntity))!;

        // Act
        INavigation entriesNavigation = Assert.Single(entityType.GetNavigations());
        IForeignKey entriesForeignKey = Assert.Single(entryEntityType.GetForeignKeys());

        // Assert
        Assert.Equal(nameof(PlaylistEntity.Entries), entriesNavigation.Name);
        Assert.Equal(typeof(PlaylistEntryEntity), entriesNavigation.TargetEntityType.ClrType);
        Assert.True(entriesNavigation.IsCollection);
        Assert.Equal(typeof(PlaylistEntity), entriesForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(PlaylistEntryEntity.PlaylistId), entriesForeignKey.Properties[0].Name);
        Assert.Equal(DeleteBehavior.Cascade, entriesForeignKey.DeleteBehavior);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheMostRecentActivityIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IIndex activityIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(PlaylistEntity.UserId),
                nameof(PlaylistEntity.UpdatedOnUtc)
            ]));

        // Assert
        Assert.False(activityIndex.IsUnique);
        Assert.Equal(2, activityIndex.Properties.Count);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldOrderEveryColumnAsConfigured()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(PlaylistEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(PlaylistEntity.Id));
        IProperty userIdProperty = entityType.GetProperty(nameof(PlaylistEntity.UserId));
        IProperty nameProperty = entityType.GetProperty(nameof(PlaylistEntity.Name));
        IProperty descriptionProperty = entityType.GetProperty(nameof(PlaylistEntity.Description));
        IProperty typeProperty = entityType.GetProperty(nameof(PlaylistEntity.Type));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(PlaylistEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(PlaylistEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(PlaylistEntity.UpdatedBy));
        IProperty versionProperty = entityType.GetProperty(nameof(PlaylistEntity.Version));
        IProperty entryCountProperty = entityType.GetProperty(nameof(PlaylistEntity.EntryCount));

        // Assert
        Assert.Equal(0, idProperty.GetColumnOrder());
        Assert.Equal(1, userIdProperty.GetColumnOrder());
        Assert.Equal(2, nameProperty.GetColumnOrder());
        Assert.Equal(3, descriptionProperty.GetColumnOrder());
        Assert.Equal(4, typeProperty.GetColumnOrder());
        Assert.Equal(5, createdOnUtcProperty.GetColumnOrder());
        Assert.Equal(6, createdByProperty.GetColumnOrder());
        Assert.Equal(7, updatedOnUtcProperty.GetColumnOrder());
        Assert.Equal(8, updatedByProperty.GetColumnOrder());
        Assert.Equal(9, versionProperty.GetColumnOrder());
        Assert.Equal(10, entryCountProperty.GetColumnOrder());
    }
}
