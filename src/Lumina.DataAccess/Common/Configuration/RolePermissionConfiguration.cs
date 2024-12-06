#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

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

        // composite primary key
        builder.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
        
        builder.Ignore(rolePermission => rolePermission.Id); // Id is only needed as a generic constraint for the repository, but this entity uses a composite key as Id

        // foreign key: Role
        builder.HasOne(rolePermission => rolePermission.Role)
            .WithMany(user => user.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // deleting a Role deletes associated RolePermission

        // foreign key: Permission
        builder.HasOne(rolePermission => rolePermission.Permission)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // deleting a Permission deletes associated RolePermission

        // audit
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

        // indexes
        builder.HasIndex(userRole => new { userRole.RoleId, userRole.PermissionId })
            .IsUnique();
    }
}
