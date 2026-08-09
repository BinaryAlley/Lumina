using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsMediaLibraryScanSnapshotsAndStagingTables : Migration
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
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<bool>(
                name: "SkipUnchangedDirectoriesDuringScan",
                table: "Libraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.CreateTable(
                name: "DirectoryScanFingerprints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    LastWriteTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryScanFingerprints", x => new { x.LibraryId, x.Path });
                    table.ForeignKey(
                        name: "FK_DirectoryScanFingerprints_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryScanSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    ContentHash = table.Column<ulong>(type: "INTEGER", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    Ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryScanSnapshots", x => new { x.LibraryId, x.Path });
                    table.ForeignKey(
                        name: "FK_LibraryScanSnapshots_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryScanStagingResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Ticks = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentHash = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PreviousContentHash = table.Column<ulong>(type: "INTEGER", nullable: false),
                    NeedsRehash = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNew = table.Column<bool>(type: "INTEGER", nullable: false),
                    LibraryScanId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryScanStagingResults", x => new { x.LibraryScanId, x.Path });
                    table.ForeignKey(
                        name: "FK_LibraryScanStagingResults_LibraryScans_LibraryScanId",
                        column: x => x.LibraryScanId,
                        principalTable: "LibraryScans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryScanFingerprints_LibraryId",
                table: "DirectoryScanFingerprints",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScanSnapshots_LibraryId",
                table: "LibraryScanSnapshots",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScanStagingResults_LibraryScanId_NeedsRehash",
                table: "LibraryScanStagingResults",
                columns: new[] { "LibraryScanId", "NeedsRehash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectoryScanFingerprints");

            migrationBuilder.DropTable(
                name: "LibraryScanSnapshots");

            migrationBuilder.DropTable(
                name: "LibraryScanStagingResults");

            migrationBuilder.DropColumn(
                name: "SkipUnchangedDirectoriesDuringScan",
                table: "Libraries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Libraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 10);
        }
    }
}
