#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="TagConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagConfigurationTests
{
    private readonly NemeraDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagConfigurationTests"/> class.
    /// </summary>
    public TagConfigurationTests()
    {
        _context = new NemeraDbContext(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TagEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Tags", tableName);
        Assert.Equal([nameof(TagEntity.Name)], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureNameAsRequired()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(TagEntity))!;

        // Act
        IProperty nameProperty = entityType.GetProperty(nameof(TagEntity.Name));

        // Assert
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(50, nameProperty.GetMaxLength());
    }
}
