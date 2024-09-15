#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="TagModel"/> entity.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<TagModel>
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Configures the <see cref="TagModel"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TagModel> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(tag => tag.Name);
        builder.Property(tag => tag.Name).IsRequired().HasMaxLength(50);
    }
    #endregion
}