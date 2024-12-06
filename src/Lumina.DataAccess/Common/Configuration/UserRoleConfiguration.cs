#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

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

        // composite primary key
        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

        builder.Ignore(userRole => userRole.Id); // Id is only needed as a generic constraint for the repository, but this entity uses a composite key as Id

        // foreign key: User
        builder.HasOne(userRole => userRole.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // deleting a User deletes associated UserRoles

        // foreign key: Role
        builder.HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // deleting a Role deletes associated UserRoles

        // audit
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

        // indexes
        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId })
            .IsUnique();
    }
}
