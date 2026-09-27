#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="TrackConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackConfigurationTests : IDisposable
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackConfigurationTests"/> class.
    /// </summary>
    public TrackConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Tracks", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValue()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(TrackEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        IProperty pathProperty = entityType.GetProperty(nameof(TrackEntity.Path));
        IProperty titleProperty = entityType.GetProperty(nameof(TrackEntity.Title));
        IProperty originalTitleProperty = entityType.GetProperty(nameof(TrackEntity.OriginalTitle));
        IProperty descriptionProperty = entityType.GetProperty(nameof(TrackEntity.Description));
        IProperty releaseCountryProperty = entityType.GetProperty(nameof(TrackEntity.ReleaseCountry));
        IProperty scriptProperty = entityType.GetProperty(nameof(TrackEntity.Script));
        IProperty keyProperty = entityType.GetProperty(nameof(TrackEntity.Key));
        IProperty audioCodecProperty = entityType.GetProperty(nameof(TrackEntity.AudioCodec));
        IProperty workProperty = entityType.GetProperty(nameof(TrackEntity.Work));

        // Assert
        Assert.False(pathProperty.IsNullable);
        Assert.Equal(2048, pathProperty.GetMaxLength());
        Assert.False(titleProperty.IsNullable);
        Assert.Equal(255, titleProperty.GetMaxLength());
        Assert.Equal(255, originalTitleProperty.GetMaxLength());
        Assert.Equal(2000, descriptionProperty.GetMaxLength());
        Assert.Equal(typeof(ReleaseCountry?), releaseCountryProperty.ClrType);
        Assert.Equal("TEXT", releaseCountryProperty.GetColumnType());
        Assert.Equal(2, releaseCountryProperty.GetMaxLength());
        Assert.Equal(50, scriptProperty.GetMaxLength());
        Assert.Equal(typeof(MusicKey?), keyProperty.ClrType);
        Assert.Equal("TEXT", keyProperty.GetColumnType());
        Assert.Equal(50, keyProperty.GetMaxLength());
        Assert.Equal(50, audioCodecProperty.GetMaxLength());
        Assert.Equal(255, workProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAlbumRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        List<IForeignKey> foreignKeys = [.. entityType.GetForeignKeys()];

        // Assert
        IForeignKey albumForeignKey = Assert.Single(foreignKeys);
        Assert.Equal(nameof(TrackEntity.AlbumId), albumForeignKey.Properties.Single().Name);
        Assert.Equal(typeof(AlbumEntity), albumForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, albumForeignKey.DeleteBehavior);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOwnedTypesForMoodsIsrcsAndRatings()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        List<IEntityType> ownedTypes = [.. _context.Model.GetEntityTypes().Where(ownedType => ownedType.IsOwned() && ownedType.FindOwnership()?.PrincipalEntityType.Name == entityType.Name)];

        // Assert
        Assert.Equal(3, ownedTypes.Count);
        Assert.Contains(ownedTypes, ownedType => ownedType.ClrType == typeof(TrackMoodEntity) && ownedType.GetTableName() == "TrackMoods");
        Assert.Contains(ownedTypes, ownedType => ownedType.ClrType == typeof(TrackIsrcEntity) && ownedType.GetTableName() == "TrackIsrcs");
        Assert.Contains(ownedTypes, ownedType => ownedType.ClrType == typeof(AudioRatingEntity) && ownedType.GetTableName() == "TrackRatings");
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureManyToManyJoinTablesForTagsAndGenres()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        List<ISkipNavigation> skipNavigations = [.. entityType.GetSkipNavigations()];

        // Assert
        Assert.Equal(2, skipNavigations.Count);
        Assert.Contains(skipNavigations, navigation => navigation.Name == nameof(TrackEntity.Tags) && navigation.JoinEntityType.GetTableName() == "TrackTags");
        Assert.Contains(skipNavigations, navigation => navigation.Name == nameof(TrackEntity.Genres) && navigation.JoinEntityType.GetTableName() == "TrackGenres");
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexesOnAlbumIdTrackNumberAndLibraryIdPath()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(TrackEntity.AlbumId), nameof(TrackEntity.TrackNumber)]));
        IIndex pathIndex = Assert.Single(indexes.Where(index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(TrackEntity.LibraryId), nameof(TrackEntity.Path)])));
        Assert.True(pathIndex.IsUnique);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(TrackEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(TrackEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(TrackEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
        Assert.True(updatedOnUtcProperty.IsNullable);
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
    }
}
