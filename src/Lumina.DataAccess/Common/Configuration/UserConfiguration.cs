#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.UsersManagement;
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

        builder.Property(user => user.TotpSecret)
            .HasColumnOrder(3);

        builder.Property(user => user.VerificationToken)
            .HasColumnOrder(4);

        builder.Property(user => user.VerificationTokenCreated)
            .HasColumnOrder(5);

        builder.HasMany(user => user.Libraries)
            .WithOne(library => library.User)
            .HasForeignKey(library => library.UserId)
            .IsRequired() // a library must have a user
            .OnDelete(DeleteBehavior.Cascade);
    }
}
