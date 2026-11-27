#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Nemera.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="RolePermissionEntity"/> entity.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
    /// <summary>
    /// Configures the <see cref="RolePermissionEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.ToTable("RolePermissions");

        // Composite primary key.
        builder.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
        
        builder.Ignore(rolePermission => rolePermission.Id); // Id is only needed as a generic constraint for the repository, but this entity uses a composite key as Id.

        // Foreign key: Role.
        builder.HasOne(rolePermission => rolePermission.Role)
            .WithMany(user => user.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // Deleting a Role deletes associated RolePermission.

        // Foreign key: Permission.
        builder.HasOne(rolePermission => rolePermission.Permission)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // Deleting a Permission deletes associated RolePermission.

        // Audit.
        builder.Property(rolePermission => rolePermission.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(rolePermission => rolePermission.CreatedBy)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(rolePermission => rolePermission.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(3);

        builder.Property(rolePermission => rolePermission.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(4);

        // Indexes.
        builder.HasIndex(userRole => new { userRole.RoleId, userRole.PermissionId })
            .IsUnique();
    }
}
