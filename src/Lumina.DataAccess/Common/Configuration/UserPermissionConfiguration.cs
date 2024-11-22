#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="UserPermissionEntity"/> entity.
/// </summary>
public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermissionEntity>
{
    /// <summary>
    /// Configures the <see cref="UserPermissionEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserPermissionEntity> builder)
    {
        builder.ToTable("UserPermissions");

        // composite primary key
        builder.HasKey(userPermission => new { userPermission.UserId, userPermission.PermissionId });

        // foreign key: User
        builder.HasOne(userPermission => userPermission.User)
            .WithMany(user => user.UserPermissions)
            .HasForeignKey(userPermission => userPermission.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // deleting a User deletes associated UserPermissions

        // foreign key: Permission
        builder.HasOne(userPermission => userPermission.Permission)
            .WithMany(role => role.UserPermissions)
            .HasForeignKey(userPermission => userPermission.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // deleting a Permission deletes associated UserPermissions

        // audit
        builder.Property(userPermission => userPermission.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(userPermission => userPermission.CreatedBy)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(userPermission => userPermission.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(3);

        builder.Property(userPermission => userPermission.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(4);

        // indexes
        builder.HasIndex(userRole => new { userRole.UserId, userRole.PermissionId })
            .IsUnique();
    }
}
