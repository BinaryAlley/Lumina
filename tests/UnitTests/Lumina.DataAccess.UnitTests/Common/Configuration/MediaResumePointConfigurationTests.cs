#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
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
/// Contains unit tests for the <see cref="MediaResumePointConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaResumePointConfigurationTests"/> class.
    /// </summary>
    public MediaResumePointConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("MediaResumePoints", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureCascadingRelationshipWithUser()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IForeignKey userForeignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(UserEntity), userForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(MediaResumePointEntity.UserId), userForeignKey.Properties[0].Name);
        Assert.Equal(DeleteBehavior.Cascade, userForeignKey.DeleteBehavior);
        Assert.True(userForeignKey.IsRequired);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldStoreTheMediaKindAsABoundedString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IProperty mediaKindProperty = entityType.GetProperty(nameof(MediaResumePointEntity.MediaKind));

        // Assert
        Assert.Equal(typeof(MediaKind), mediaKindProperty.ClrType);
        Assert.Equal(typeof(string), mediaKindProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", mediaKindProperty.GetColumnType());
        Assert.Equal(20, mediaKindProperty.GetMaxLength());
        Assert.False(mediaKindProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldStoreThePositionKindAsABoundedRequiredString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IProperty lastPositionKindProperty = entityType.GetProperty(nameof(MediaResumePointEntity.LastPositionKind));

        // Assert
        Assert.Equal(typeof(MediaPositionKind), lastPositionKindProperty.ClrType);
        Assert.Equal(typeof(string), lastPositionKindProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", lastPositionKindProperty.GetColumnType());
        Assert.Equal(20, lastPositionKindProperty.GetMaxLength());
        Assert.False(lastPositionKindProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldBoundThePositionLocationLength()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IProperty lastPositionLocationProperty = entityType.GetProperty(nameof(MediaResumePointEntity.LastPositionLocation));

        // Assert
        Assert.Equal(typeof(string), lastPositionLocationProperty.ClrType);
        Assert.Equal(4096, lastPositionLocationProperty.GetMaxLength());
        Assert.True(lastPositionLocationProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheRequiredColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IProperty mediaKindProperty = entityType.GetProperty(nameof(MediaResumePointEntity.MediaKind));
        IProperty lastPositionKindProperty = entityType.GetProperty(nameof(MediaResumePointEntity.LastPositionKind));
        IProperty lastProgressFractionProperty = entityType.GetProperty(nameof(MediaResumePointEntity.LastProgressFraction));
        IProperty lastConsumedOnUtcProperty = entityType.GetProperty(nameof(MediaResumePointEntity.LastConsumedOnUtc));
        IProperty versionProperty = entityType.GetProperty(nameof(MediaResumePointEntity.Version));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(MediaResumePointEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(MediaResumePointEntity.CreatedBy));

        // Assert
        Assert.False(mediaKindProperty.IsNullable);
        Assert.False(lastPositionKindProperty.IsNullable);
        Assert.False(lastProgressFractionProperty.IsNullable);
        Assert.False(lastConsumedOnUtcProperty.IsNullable);
        Assert.False(versionProperty.IsNullable);
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheVersionAsAConcurrencyToken()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IProperty versionProperty = entityType.GetProperty(nameof(MediaResumePointEntity.Version));

        // Assert
        Assert.True(versionProperty.IsConcurrencyToken);
        Assert.False(versionProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheUniqueBusinessKeyIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IIndex businessKeyIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(MediaResumePointEntity.UserId),
                nameof(MediaResumePointEntity.MediaKind),
                nameof(MediaResumePointEntity.MediaItemId)
            ]));

        // Assert
        Assert.True(businessKeyIndex.IsUnique);
        Assert.Equal(3, businessKeyIndex.Properties.Count);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaResumePointEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(MediaResumePointEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(MediaResumePointEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(MediaResumePointEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(MediaResumePointEntity.UpdatedBy));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
        Assert.Null(updatedByProperty.GetDefaultValue());
    }
}
