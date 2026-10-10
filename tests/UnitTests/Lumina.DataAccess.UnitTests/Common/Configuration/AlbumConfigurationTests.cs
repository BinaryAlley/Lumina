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
/// Contains unit tests for the <see cref="AlbumConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumConfigurationTests : IDisposable
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumConfigurationTests"/> class.
    /// </summary>
    public AlbumConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(AlbumEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Albums", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValue()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(AlbumEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(AlbumEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(AlbumEntity))!;

        // Act
        IProperty artistIdProperty = entityType.GetProperty(nameof(AlbumEntity.ArtistId));
        IProperty libraryIdProperty = entityType.GetProperty(nameof(AlbumEntity.LibraryId));
        IProperty titleProperty = entityType.GetProperty(nameof(AlbumEntity.Title));
        IProperty originalTitleProperty = entityType.GetProperty(nameof(AlbumEntity.OriginalTitle));
        IProperty descriptionProperty = entityType.GetProperty(nameof(AlbumEntity.Description));
        IProperty releaseCountryProperty = entityType.GetProperty(nameof(AlbumEntity.ReleaseCountry));
        IProperty releaseTypeProperty = entityType.FindNavigation(nameof(AlbumEntity.ReleaseTypes))!.TargetEntityType.GetProperty(nameof(AlbumReleaseTypeEntity.ReleaseType));
        IProperty releaseStatusProperty = entityType.GetProperty(nameof(AlbumEntity.ReleaseStatus));
        IProperty mediaFormatProperty = entityType.GetProperty(nameof(AlbumEntity.MediaFormat));
        IProperty barcodeProperty = entityType.GetProperty(nameof(AlbumEntity.Barcode));
        IProperty catalogNumberProperty = entityType.FindNavigation(nameof(AlbumEntity.CatalogNumbers))!.TargetEntityType.GetProperty(nameof(AlbumCatalogNumberEntity.CatalogNumber));
        IProperty languageCodeProperty = entityType.GetProperty(nameof(AlbumEntity.LanguageCode));

        // Assert
        Assert.False(artistIdProperty.IsNullable);
        Assert.False(libraryIdProperty.IsNullable);
        Assert.False(titleProperty.IsNullable);
        Assert.Equal(255, titleProperty.GetMaxLength());
        Assert.Equal(255, originalTitleProperty.GetMaxLength());
        Assert.Equal(2000, descriptionProperty.GetMaxLength());
        Assert.Equal(typeof(ReleaseCountry?), releaseCountryProperty.ClrType);
        Assert.Equal("TEXT", releaseCountryProperty.GetColumnType());
        Assert.Equal(2, releaseCountryProperty.GetMaxLength());
        Assert.Equal(typeof(MusicReleaseType), releaseTypeProperty.ClrType);
        Assert.Equal("TEXT", releaseTypeProperty.GetColumnType());
        Assert.Equal(50, releaseTypeProperty.GetMaxLength());
        Assert.Equal(typeof(MusicReleaseStatus?), releaseStatusProperty.ClrType);
        Assert.Equal("TEXT", releaseStatusProperty.GetColumnType());
        Assert.Equal(50, releaseStatusProperty.GetMaxLength());
        Assert.Equal(typeof(MusicMediaFormat?), mediaFormatProperty.ClrType);
        Assert.Equal("TEXT", mediaFormatProperty.GetColumnType());
        Assert.Equal(50, mediaFormatProperty.GetMaxLength());
        Assert.Equal(13, barcodeProperty.GetMaxLength());
        Assert.Equal(50, catalogNumberProperty.GetMaxLength());
        Assert.True(languageCodeProperty.IsNullable);
        Assert.Equal(typeof(string), languageCodeProperty.ClrType);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureArtistRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(AlbumEntity))!;

        // Act
        List<IForeignKey> foreignKeys = [.. entityType.GetForeignKeys()];

        // Assert
        IForeignKey artistForeignKey = Assert.Single(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(ArtistEntity));
        Assert.Equal(nameof(AlbumEntity.ArtistId), artistForeignKey.Properties.Single().Name);
        Assert.Equal(DeleteBehavior.Cascade, artistForeignKey.DeleteBehavior);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOwnedAlbumRatings()
    {
        // Arrange
        IEntityType ratingEntityType = _context.Model.GetEntityTypes()
            .Single(entityType => entityType.IsOwned() && entityType.ClrType == typeof(AudioRatingEntity) && entityType.GetTableName() == "AlbumRatings");

        // Act
        string? tableName = ratingEntityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = ratingEntityType.FindPrimaryKey()!.Properties;
        IProperty valueProperty = ratingEntityType.GetProperty(nameof(AudioRatingEntity.Value));
        IProperty maxValueProperty = ratingEntityType.GetProperty(nameof(AudioRatingEntity.MaxValue));
        IProperty voteCountProperty = ratingEntityType.GetProperty(nameof(AudioRatingEntity.VoteCount));
        IProperty sourceProperty = ratingEntityType.GetProperty(nameof(AudioRatingEntity.Source));

        // Assert
        Assert.Equal("AlbumRatings", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
        Assert.Equal("decimal(3,2)", valueProperty.GetColumnType());
        Assert.False(valueProperty.IsNullable);
        Assert.Equal("decimal(3,2)", maxValueProperty.GetColumnType());
        Assert.False(maxValueProperty.IsNullable);
        Assert.True(voteCountProperty.IsNullable);
        Assert.Equal(typeof(AudioRatingSource?), sourceProperty.ClrType);
        Assert.Equal("TEXT", sourceProperty.GetColumnType());
        Assert.Equal(50, sourceProperty.GetMaxLength());
        Assert.True(sourceProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureManyToManyJoinTablesForTagsAndGenres()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(AlbumEntity))!;

        // Act
        List<ISkipNavigation> skipNavigations = [.. entityType.GetSkipNavigations()];

        // Assert
        Assert.Equal(2, skipNavigations.Count);
        ISkipNavigation tagsNavigation = Assert.Single(skipNavigations, navigation => navigation.Name == nameof(AlbumEntity.Tags));
        Assert.Equal("AlbumTags", tagsNavigation.JoinEntityType.GetTableName());
        Assert.Equal(["AlbumId", "TagId"], tagsNavigation.JoinEntityType.FindPrimaryKey()!.Properties.Select(property => property.Name));
        ISkipNavigation genresNavigation = Assert.Single(skipNavigations, navigation => navigation.Name == nameof(AlbumEntity.Genres));
        Assert.Equal("AlbumGenres", genresNavigation.JoinEntityType.GetTableName());
        Assert.Equal(["AlbumId", "GenreId"], genresNavigation.JoinEntityType.FindPrimaryKey()!.Properties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexesOnArtistIdTitleAndLibraryIdTitle()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(AlbumEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(AlbumEntity.ArtistId), nameof(AlbumEntity.Title)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(AlbumEntity.LibraryId), nameof(AlbumEntity.Title)]));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(AlbumEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(AlbumEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(AlbumEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(AlbumEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(AlbumEntity.UpdatedBy));

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
