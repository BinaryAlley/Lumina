#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="TrackContributorConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackContributorConfigurationTests : IDisposable
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackContributorConfigurationTests"/> class.
    /// </summary>
    public TrackContributorConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackContributorEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("TrackContributors", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValue()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackContributorEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(TrackContributorEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackContributorEntity))!;

        // Act
        IProperty trackIdProperty = entityType.GetProperty(nameof(TrackContributorEntity.TrackId));
        IProperty mediaContributorIdProperty = entityType.GetProperty(nameof(TrackContributorEntity.MediaContributorId));
        IProperty roleProperty = entityType.GetProperty(nameof(TrackContributorEntity.Role));

        // Assert
        Assert.False(trackIdProperty.IsNullable);
        Assert.False(mediaContributorIdProperty.IsNullable);
        Assert.False(roleProperty.IsNullable);
        Assert.Equal(typeof(MediaContributorRole), roleProperty.ClrType);
        Assert.Equal("TEXT", roleProperty.GetColumnType());
        Assert.Equal(50, roleProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueIndexAndContributorLookupIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackContributorEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        IIndex uniquenessIndex = Assert.Single(indexes.Where(index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(TrackContributorEntity.TrackId), nameof(TrackContributorEntity.MediaContributorId), nameof(TrackContributorEntity.Role)])));
        Assert.True(uniquenessIndex.IsUnique);
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(TrackContributorEntity.MediaContributorId)]));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TrackContributorEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(TrackContributorEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(TrackContributorEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(TrackContributorEntity.UpdatedOnUtc));

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
