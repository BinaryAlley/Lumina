#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="PermissionEntity"/> entity.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    /// <summary>
    /// Configures the <see cref="PermissionEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Id)
            .ValueGeneratedOnAdd()
            .HasColumnOrder(0);

        builder.Property(permission => permission.PermissionName)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100)
            .HasColumnOrder(1);

        // audit
        builder.Property(permission => permission.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(permission => permission.CreatedBy)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(permission => permission.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(5);

        builder.Property(permission => permission.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(6);
    }
}
