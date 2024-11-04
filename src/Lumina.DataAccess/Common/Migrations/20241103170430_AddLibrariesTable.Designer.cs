﻿// <auto-generated />
using System;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    [DbContext(typeof(LuminaDbContext))]
    [Migration("20241103170430_AddLibrariesTable")]
    partial class AddLibrariesTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("BookGenres", b =>
                {
                    b.Property<Guid>("BookId")
                        .HasColumnType("TEXT");

                    b.Property<string>("GenreId")
                        .HasColumnType("TEXT");

                    b.HasKey("BookId", "GenreId");

                    b.HasIndex("GenreId");

                    b.ToTable("BookGenres", (string)null);
                });

            modelBuilder.Entity("BookTags", b =>
                {
                    b.Property<Guid>("BookId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TagId")
                        .HasColumnType("TEXT");

                    b.HasKey("BookId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("BookTags", (string)null);
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.Common.GenreEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("Genres", (string)null);
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.Common.TagEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("Tags", (string)null);
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management.LibraryEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(0);

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("LibraryType")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(2);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(1);

                    b.Property<DateTime?>("Updated")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Libraries", (string)null);
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary.BookEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(0);

                    b.Property<string>("ASIN")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(21);

                    b.Property<string>("AppleBooksId")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(29);

                    b.Property<string>("BarnesAndNobleId")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(28);

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(30);

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(3);

                    b.Property<string>("Edition")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(19);

                    b.Property<string>("Format")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(18);

                    b.Property<string>("GoodreadsId")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(22);

                    b.Property<string>("GoogleBooksId")
                        .HasMaxLength(12)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(27);

                    b.Property<string>("LCCN")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(23);

                    b.Property<string>("LanguageCode")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(10);

                    b.Property<string>("LanguageName")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(11);

                    b.Property<string>("LanguageNativeName")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(12);

                    b.Property<string>("LibraryThingId")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(26);

                    b.Property<string>("OCLCNumber")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(24);

                    b.Property<string>("OpenLibraryId")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(25);

                    b.Property<string>("OriginalLanguageCode")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(13);

                    b.Property<string>("OriginalLanguageName")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(14);

                    b.Property<string>("OriginalLanguageNativeName")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(15);

                    b.Property<DateOnly?>("OriginalReleaseDate")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(4);

                    b.Property<int?>("OriginalReleaseYear")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(5);

                    b.Property<string>("OriginalTitle")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(2);

                    b.Property<int?>("PageCount")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(17);

                    b.Property<string>("Publisher")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(16);

                    b.Property<DateOnly?>("ReReleaseDate")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(6);

                    b.Property<int?>("ReReleaseYear")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(7);

                    b.Property<string>("ReleaseCountry")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(8);

                    b.Property<string>("ReleaseVersion")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(9);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(1);

                    b.Property<DateTime?>("Updated")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(31);

                    b.Property<int?>("VolumeNumber")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(20);

                    b.HasKey("Id");

                    b.ToTable("Books", (string)null);
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.UsersManagement.UserEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(0);

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(2);

                    b.Property<string>("TotpSecret")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(3);

                    b.Property<DateTime?>("Updated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasColumnOrder(1);

                    b.Property<string>("VerificationToken")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(4);

                    b.Property<DateTime?>("VerificationTokenCreated")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(5);

                    b.HasKey("Id");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("BookGenres", b =>
                {
                    b.HasOne("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary.BookEntity", null)
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Lumina.Application.Common.DataAccess.Entities.Common.GenreEntity", null)
                        .WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BookTags", b =>
                {
                    b.HasOne("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary.BookEntity", null)
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Lumina.Application.Common.DataAccess.Entities.Common.TagEntity", null)
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management.LibraryEntity", b =>
                {
                    b.HasOne("Lumina.Application.Common.DataAccess.Entities.UsersManagement.UserEntity", "User")
                        .WithMany("Libraries")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management.LibraryContentLocationEntity", "ContentLocations", b1 =>
                        {
                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("LibraryId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Path")
                                .IsRequired()
                                .HasMaxLength(260)
                                .HasColumnType("TEXT")
                                .HasColumnName("Path");

                            b1.HasKey("Id");

                            b1.HasIndex("LibraryId");

                            b1.ToTable("LibraryContentLocations", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("LibraryId");
                        });

                    b.Navigation("ContentLocations");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary.BookEntity", b =>
                {
                    b.OwnsMany("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary.BookRatingEntity", "Ratings", b1 =>
                        {
                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("BookId")
                                .HasColumnType("TEXT");

                            b1.Property<decimal?>("MaxValue")
                                .IsRequired()
                                .HasColumnType("decimal(3,2)");

                            b1.Property<string>("Source")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("TEXT");

                            b1.Property<decimal?>("Value")
                                .IsRequired()
                                .HasColumnType("decimal(3,2)");

                            b1.Property<int?>("VoteCount")
                                .HasColumnType("INTEGER");

                            b1.HasKey("Id");

                            b1.HasIndex("BookId");

                            b1.ToTable("BookRatings", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("BookId");
                        });

                    b.OwnsMany("Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary.IsbnEntity", "ISBNs", b1 =>
                        {
                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("BookId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Format")
                                .IsRequired()
                                .HasMaxLength(6)
                                .HasColumnType("TEXT");

                            b1.Property<string>("Value")
                                .IsRequired()
                                .HasMaxLength(13)
                                .HasColumnType("TEXT")
                                .HasColumnName("ISBN");

                            b1.HasKey("Id");

                            b1.HasIndex("BookId");

                            b1.ToTable("BookISBNs", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("BookId");
                        });

                    b.Navigation("ISBNs");

                    b.Navigation("Ratings");
                });

            modelBuilder.Entity("Lumina.Application.Common.DataAccess.Entities.UsersManagement.UserEntity", b =>
                {
                    b.Navigation("Libraries");
                });
#pragma warning restore 612, 618
        }
    }
}
