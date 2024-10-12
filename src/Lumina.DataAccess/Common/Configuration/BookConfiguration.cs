#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Models.Common;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="BookModel"/> entity.
/// </summary>
public class BookConfiguration : IEntityTypeConfiguration<BookModel>
{
    /// <summary>
    /// Configures the <see cref="BookModel"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<BookModel> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(book => book.Id);
        builder.Property(book => book.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(book => book.Title).IsRequired().HasMaxLength(255).HasColumnOrder(1);
        builder.Property(book => book.OriginalTitle).HasMaxLength(255).HasColumnOrder(2);
        builder.Property(book => book.Description).HasMaxLength(2000).HasColumnOrder(3);
        builder.Property(book => book.OriginalReleaseDate).HasColumnOrder(4);
        builder.Property(book => book.OriginalReleaseYear).HasColumnOrder(5);
        builder.Property(book => book.ReReleaseDate).HasColumnOrder(6);
        builder.Property(book => book.ReReleaseYear).HasColumnOrder(7);
        builder.Property(book => book.ReleaseCountry).HasColumnOrder(8);
        builder.Property(book => book.ReleaseVersion).HasColumnOrder(9);
        builder.Property(book => book.LanguageCode).HasColumnOrder(10);
        builder.Property(book => book.LanguageName).HasColumnOrder(11);
        builder.Property(book => book.LanguageNativeName).HasColumnOrder(12);
        builder.Property(book => book.OriginalLanguageCode).HasColumnOrder(13);
        builder.Property(book => book.OriginalLanguageName).HasColumnOrder(14);
        builder.Property(book => book.OriginalLanguageNativeName).HasColumnOrder(15);
        // since Tag is a Domain ValueObject (no identity), but we also don't want to have duplicates,
        // we need to configure it as a many-to-many relationship, where the tag itself is the primary key
        builder.HasMany(book => book.Tags)
        .WithMany()
        .UsingEntity<Dictionary<string, object>>(
            "BookTags",
            j => j.HasOne<TagModel>().WithMany().HasForeignKey("TagId"),
            j => j.HasOne<BookModel>().WithMany().HasForeignKey("BookId"),
            j =>
            {
                j.HasKey("BookId", "TagId");
                j.ToTable("BookTags");
            });

        builder.HasMany(book => book.Genres)
        .WithMany()
        .UsingEntity<Dictionary<string, object>>(
            "BookGenres",
            j => j.HasOne<GenreModel>().WithMany().HasForeignKey("GenreId"),
            j => j.HasOne<BookModel>().WithMany().HasForeignKey("BookId"),
            j =>
            {
                j.HasKey("BookId", "GenreId");
                j.ToTable("BookGenres");
            });

        builder.Property(book => book.Publisher).HasMaxLength(100).HasColumnOrder(16);
        builder.Property(book => book.PageCount).HasColumnOrder(17);
        builder.Property(book => book.Format).HasColumnOrder(18);
        builder.Property(book => book.Edition).HasMaxLength(50).HasColumnOrder(19);
        builder.Property(book => book.VolumeNumber).HasColumnOrder(20);
        builder.Property(book => book.ASIN).HasMaxLength(10).HasColumnOrder(21);
        builder.Property(book => book.GoodreadsId).HasColumnOrder(22);
        builder.Property(book => book.LCCN).HasColumnOrder(23);
        builder.Property(book => book.OCLCNumber).HasColumnOrder(24);
        builder.Property(book => book.OpenLibraryId).HasMaxLength(50).HasColumnOrder(25);
        builder.Property(book => book.LibraryThingId).HasMaxLength(50).HasColumnOrder(26);
        builder.Property(book => book.GoogleBooksId).HasMaxLength(12).HasColumnOrder(27);
        builder.Property(book => book.BarnesAndNobleId).HasMaxLength(10).HasColumnOrder(28);
        builder.Property(book => book.AppleBooksId).HasColumnOrder(29);
        builder.Property(book => book.Created).HasColumnOrder(30);
        builder.Property(book => book.Updated).HasColumnOrder(31);

        //builder.HasMany<ContributorIdModel>()
        //.WithMany()
        //.UsingEntity<Dictionary<string, object>>(
        //    "BookContributors",
        //    j => j.HasOne<ContributorIdModel>().WithMany().HasForeignKey("ContributorId"),
        //    j => j.HasOne<BookModel>().WithMany().HasForeignKey("BookId"),
        //    j =>
        //    {
        //        j.HasKey("BookId", "ContributorId");
        //        j.ToTable("BookContributors");
        //    });

        //builder.OwnsMany(book => book.ContributorIds, ownedBuilder =>
        //{
        //    ownedBuilder.ToTable("BookContributors");
        //    ownedBuilder.WithOwner().HasForeignKey("BookId");
        //    ownedBuilder.Property(c => c.Id)
        //        .HasColumnName("ContributorId")
        //        .HasColumnType("uniqueidentifier");
        //    ownedBuilder.HasKey("BookId", "Id"); // Use the actual property name 'Id' here
        //});

        builder.OwnsMany(book => book.Ratings, ratingBuilder =>
        {
            ratingBuilder.ToTable("BookRatings");
            ratingBuilder.WithOwner().HasForeignKey("BookId");
            ratingBuilder.Property<int>("Id").ValueGeneratedOnAdd();
            ratingBuilder.HasKey("Id");

            ratingBuilder.Property(rating => rating.Value)
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            ratingBuilder.Property(rating => rating.MaxValue)
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            ratingBuilder.Property(rating => rating.VoteCount)
                .IsRequired(false);

            ratingBuilder.Property(rating => rating.Source)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.OwnsMany(book => book.ISBNs, isbnBuilder =>
        {
            isbnBuilder.ToTable("BookISBNs");
            isbnBuilder.WithOwner().HasForeignKey("BookId");
            isbnBuilder.Property<int>("Id").ValueGeneratedOnAdd();
            isbnBuilder.HasKey("Id");

            isbnBuilder.Property(isbn => isbn.Value)
                .HasColumnName("ISBN")
                .HasMaxLength(13)
                .IsRequired();

            isbnBuilder.Property(isbn => isbn.Format)
                .HasConversion<string>()
                .HasMaxLength(6)
                .IsRequired();
        });
    }
}

//public class ContributorConfiguration : IEntityTypeConfiguration<ContributorIdModel>
//{
//    public void Configure(EntityTypeBuilder<ContributorIdModel> builder)
//    {
//        builder.ToTable("Contributors");
//        builder.HasKey(c => c.Id);
//        builder.Property(c => c.Id).ValueGeneratedNever();
//    }
//}