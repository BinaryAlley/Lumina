#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="GenreEntity"/> entity.
/// </summary>
public class GenreConfiguration : IEntityTypeConfiguration<GenreEntity>
{
    /// <summary>
    /// Configures the <see cref="GenreEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<GenreEntity> builder)
    {
        builder.ToTable("Genres");
        builder.HasKey(genre => genre.Name);
        builder.Property(genre => genre.Name).IsRequired().HasMaxLength(50);
    }
}