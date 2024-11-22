#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="RoleEntity"/> entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    /// <summary>
    /// Configures the <see cref="RoleEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id)
            .ValueGeneratedOnAdd() 
            .HasColumnOrder(0);

        builder.Property(role => role.RoleName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnOrder(1);

        // audit
        builder.Property(role => role.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(role => role.CreatedBy)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(role => role.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(4);

        builder.Property(role => role.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(5);
    }
}
