using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsBookLibraryAndPathAndMetadataStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMetadataUpdateUtc",
                table: "Books",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 39);

            migrationBuilder.AddColumn<Guid>(
                name: "LibraryId",
                table: "Books",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
                .Annotation("Relational:ColumnOrder", 36);

            migrationBuilder.AddColumn<string>(
                name: "MetadataProvider",
                table: "Books",
                type: "TEXT",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 40);

            migrationBuilder.AddColumn<string>(
                name: "MetadataStatus",
                table: "Books",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 38);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Books",
                type: "TEXT",
                maxLength: 2048,
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 37);

            migrationBuilder.CreateIndex(
                name: "IX_Books_LibraryId_MetadataStatus",
                table: "Books",
                columns: new[] { "LibraryId", "MetadataStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Books_LibraryId_Path",
                table: "Books",
                columns: new[] { "LibraryId", "Path" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_LibraryId_MetadataStatus",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_LibraryId_Path",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "LastMetadataUpdateUtc",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "LibraryId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "MetadataProvider",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "MetadataStatus",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Books");
        }
    }
}
