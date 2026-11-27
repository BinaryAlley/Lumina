using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nemera.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsBookDeletionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Books",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 42);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 41);

            // This migration introduces both deletion columns together, so a database coming from an older schema has no deleted books to backfill.
            // The statement is kept for databases that were created by an intermediate build that already had IsDeleted.
            migrationBuilder.Sql("""
                UPDATE Books
                SET DeletedOnUtc = COALESCE(UpdatedOnUtc, CreatedOnUtc)
                WHERE IsDeleted = 1 AND DeletedOnUtc IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Books_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Books",
                columns: new[] { "LibraryId", "IsDeleted", "DeletedOnUtc", "Title", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Books");
        }
    }
}
