#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="BookConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookConfigurationTests"/> class.
    /// </summary>
    public BookConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(BookEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Books", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(BookEntity))!;

        // Act
        IProperty titleProperty = entityType.GetProperty(nameof(BookEntity.Title));
        IProperty pathProperty = entityType.GetProperty(nameof(BookEntity.Path));
        IProperty formatProperty = entityType.GetProperty(nameof(BookEntity.Format));
        IProperty metadataStatusProperty = entityType.GetProperty(nameof(BookEntity.MetadataStatus));

        // Assert
        Assert.False(titleProperty.IsNullable);
        Assert.Equal(255, titleProperty.GetMaxLength());
        Assert.False(pathProperty.IsNullable);
        Assert.Equal(2048, pathProperty.GetMaxLength());
        Assert.Equal(typeof(BookFormat?), formatProperty.ClrType);
        Assert.Equal("TEXT", formatProperty.GetColumnType());
        Assert.Equal(typeof(MetadataStatus), metadataStatusProperty.ClrType);
        Assert.Equal("TEXT", metadataStatusProperty.GetColumnType());
        Assert.Equal(20, metadataStatusProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOwnedTypesForRatingsAndIsbns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(BookEntity))!;

        // Act
        List<IEntityType> ownedTypes = [.. _context.Model.GetEntityTypes().Where(ownedType => ownedType.IsOwned() && ownedType.FindOwnership()?.PrincipalEntityType.Name == entityType.Name)];

        // Assert
        Assert.Equal(2, ownedTypes.Count);
        Assert.Contains(ownedTypes, ownedType => ownedType.ClrType == typeof(BookRatingEntity) && ownedType.GetTableName() == "BookRatings");
        Assert.Contains(ownedTypes, ownedType => ownedType.ClrType == typeof(IsbnEntity) && ownedType.GetTableName() == "BookISBNs");
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureManyToManyJoinTablesForTagsAndGenres()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(BookEntity))!;

        // Act
        List<ISkipNavigation> skipNavigations = [.. entityType.GetSkipNavigations()];

        // Assert
        Assert.Equal(2, skipNavigations.Count);
        Assert.Contains(skipNavigations, navigation => navigation.Name == nameof(BookEntity.Tags) && navigation.JoinEntityType.GetTableName() == "BookTags");
        Assert.Contains(skipNavigations, navigation => navigation.Name == nameof(BookEntity.Genres) && navigation.JoinEntityType.GetTableName() == "BookGenres");
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureIndexesOnLibraryIdAndPath()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(BookEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(BookEntity.LibraryId), nameof(BookEntity.Path)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(BookEntity.LibraryId), nameof(BookEntity.MetadataStatus)]));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(BookEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(BookEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(BookEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
