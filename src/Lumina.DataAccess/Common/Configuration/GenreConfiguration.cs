#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="GenreDto"/> entity.
/// </summary>
public class GenreConfiguration : IEntityTypeConfiguration<GenreDto>
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Configures the <see cref="GenreDto"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<GenreDto> builder)
    {
        builder.ToTable("Genres");
        builder.HasKey(genre => genre.Name);
        builder.Property(genre => genre.Name).IsRequired().HasMaxLength(50);
    }
    #endregion
}