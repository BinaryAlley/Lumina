#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Nemera.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="UserRoleEntity"/> entity.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    /// <summary>
    /// Configures the <see cref="UserRoleEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.ToTable("UserRoles");

        // Primary key.
        builder.HasKey(userRole => userRole.UserId );

        builder.Ignore(userRole => userRole.Id); // Id is only needed as a generic constraint for the repository, but this entity uses a composite key as Id.

        // Foreign key: User (one-to-one relationship).
        builder.HasOne(userRole => userRole.User)
            .WithOne(user => user.UserRole)
            .HasForeignKey<UserRoleEntity>(userRole => userRole.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // Deleting a User deletes associated UserRole.

        // Foreign key: Role (many-to-one relationship).
        builder.HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // Deleting a Role deletes associated UserRoles.

        // Audit.
        builder.Property(userRole => userRole.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(userRole => userRole.CreatedBy)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(userRole => userRole.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(3);

        builder.Property(userRole => userRole.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(4);

        // Indexes.
        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId })
            .IsUnique();
    }
}
