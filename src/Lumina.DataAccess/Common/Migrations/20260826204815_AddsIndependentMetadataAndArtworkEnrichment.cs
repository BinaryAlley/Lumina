using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsIndependentMetadataAndArtworkEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Libraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AddColumn<string>(
                name: "ArtworkProvidersConfigurationFingerprint",
                table: "Libraries",
                type: "TEXT",
                maxLength: 64,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AddColumn<string>(
                name: "MetadataProvidersConfigurationFingerprint",
                table: "Libraries",
                type: "TEXT",
                maxLength: 64,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.CreateTable(
                name: "BookArtwork",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtworkType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    ContentHash = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastUpdateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookArtwork", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookArtwork_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // backfill the artwork of the books that already had a cover, so that the stored covers are preserved as enriched cover artwork
            // before the CoverImagePath column is dropped
            migrationBuilder.Sql("""
                INSERT INTO BookArtwork (Id, BookId, ArtworkType, Ordinal, FileName, ContentHash, Status, Provider, LastUpdateUtc, CreatedOnUtc, CreatedBy, UpdatedOnUtc, UpdatedBy)
                SELECT lower(hex(randomblob(16))), Id, 'Cover', 0, CoverImagePath, 0, 'Enriched', NULL, NULL, datetime('now'), '00000000-0000-0000-0000-000000000000', NULL, NULL
                FROM Books
                WHERE CoverImagePath IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "BookContributors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaContributorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RoleCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookContributors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookContributors_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaContributors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, collation: "NOCASE"),
                    LegalName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Biography = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DateOfDeath = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaContributors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookArtwork_BookId_ArtworkType_Ordinal",
                table: "BookArtwork",
                columns: new[] { "BookId", "ArtworkType", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookArtwork_Status",
                table: "BookArtwork",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BookContributors_BookId_MediaContributorId",
                table: "BookContributors",
                columns: new[] { "BookId", "MediaContributorId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookContributors_MediaContributorId",
                table: "BookContributors",
                column: "MediaContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaContributors_DisplayName",
                table: "MediaContributors",
                column: "DisplayName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookArtwork");

            migrationBuilder.DropTable(
                name: "BookContributors");

            migrationBuilder.DropTable(
                name: "MediaContributors");

            migrationBuilder.DropColumn(
                name: "ArtworkProvidersConfigurationFingerprint",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "MetadataProvidersConfigurationFingerprint",
                table: "Libraries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Libraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "Books",
                type: "TEXT",
                nullable: true);
        }
    }
}
