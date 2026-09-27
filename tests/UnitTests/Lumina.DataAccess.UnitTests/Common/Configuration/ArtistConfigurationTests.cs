#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
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
/// Contains unit tests for the <see cref="ArtistConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistConfigurationTests : IDisposable
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistConfigurationTests"/> class.
    /// </summary>
    public ArtistConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(ArtistEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Artists", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValue()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(ArtistEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(ArtistEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(ArtistEntity))!;

        // Act
        IProperty libraryIdProperty = entityType.GetProperty(nameof(ArtistEntity.LibraryId));
        IProperty nameProperty = entityType.GetProperty(nameof(ArtistEntity.Name));
        IProperty websiteProperty = entityType.GetProperty(nameof(ArtistEntity.Website));
        IProperty musicBrainzArtistIdProperty = entityType.GetProperty(nameof(ArtistEntity.MusicBrainzArtistId));

        // Assert
        Assert.False(libraryIdProperty.IsNullable);
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(255, nameProperty.GetMaxLength());
        Assert.True(websiteProperty.IsNullable);
        Assert.Equal(2048, websiteProperty.GetMaxLength());
        Assert.True(musicBrainzArtistIdProperty.IsNullable);
        Assert.Equal(typeof(Guid?), musicBrainzArtistIdProperty.ClrType);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureContributorsRelationshipWithCascadeDelete()
    {
        // Arrange
        IEntityType contributorEntityType = _context.Model.FindEntityType(typeof(ArtistContributorEntity))!;

        // Act
        List<IForeignKey> foreignKeys = [.. contributorEntityType.GetForeignKeys()];

        // Assert
        IForeignKey artistForeignKey = Assert.Single(foreignKeys, foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(ArtistEntity));
        Assert.Equal(nameof(ArtistContributorEntity.ArtistId), artistForeignKey.Properties.Single().Name);
        Assert.Equal(DeleteBehavior.Cascade, artistForeignKey.DeleteBehavior);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueIndexOnLibraryIdAndName()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(ArtistEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        IIndex uniquenessIndex = Assert.Single(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(ArtistEntity.LibraryId), nameof(ArtistEntity.Name)]));
        Assert.True(uniquenessIndex.IsUnique);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(ArtistEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(ArtistEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(ArtistEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(ArtistEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(ArtistEntity.UpdatedBy));

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
