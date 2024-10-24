#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="TagEntity"/> entity.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    /// <summary>
    /// Configures the <see cref="TagEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(tag => tag.Name);
        builder.Property(tag => tag.Name).IsRequired().HasMaxLength(50);
    }
}