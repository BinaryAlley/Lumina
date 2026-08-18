#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
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
/// Contains unit tests for the <see cref="GenreConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenreConfigurationTests"/> class.
    /// </summary>
    public GenreConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(GenreEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Genres", tableName);
        Assert.Equal([nameof(GenreEntity.Name)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureNameAsRequired()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(GenreEntity))!;

        // Act
        IProperty nameProperty = entityType.GetProperty(nameof(GenreEntity.Name));

        // Assert
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(50, nameProperty.GetMaxLength());
    }
}
