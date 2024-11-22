#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="UserEntity"/> entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <summary>
    /// Configures the <see cref="UserEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(user => user.Username)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(1);

        builder.Property(user => user.Password)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(user => user.TempPassword)
            .HasDefaultValue(null)
            .HasColumnOrder(3);

        builder.Property(user => user.TotpSecret)
            .HasDefaultValue(null)
            .HasColumnOrder(4);

        builder.Property(user => user.TempPasswordCreated)
            .HasDefaultValue(null)
            .HasColumnOrder(5);

        // relationships
        builder.HasMany(user => user.Libraries)
            .WithOne(library => library.User)
            .HasForeignKey(library => library.UserId)
            .IsRequired() // a library must have a user
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.UserRoles)
            .WithOne(userRole => userRole.User)
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasMany(user => user.UserPermissions)
            .WithOne(userPermission => userPermission.User)
            .HasForeignKey(userPermission => userPermission.UserId)
            .OnDelete(DeleteBehavior.Cascade); 

        // audit
        builder.Property(user => user.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(user => user.CreatedBy)
            .IsRequired()
            .HasColumnOrder(7);

        builder.Property(user => user.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        builder.Property(user => user.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(9);

        // indexes
        builder.HasIndex(user => user.Username)
            .IsUnique(); // queried frequently
    }
}
