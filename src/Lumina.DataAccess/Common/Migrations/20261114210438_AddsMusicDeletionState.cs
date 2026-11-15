using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsMusicDeletionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Tracks",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 51);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Artists",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 21);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Artists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Albums",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 40);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Albums",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 39);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Tracks",
                columns: new[] { "LibraryId", "IsDeleted", "DeletedOnUtc", "Title", "Id" },
                filter: "\"IsDeleted\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_LibraryId_IsDeleted_DeletedOnUtc_Name_Id",
                table: "Artists",
                columns: new[] { "LibraryId", "IsDeleted", "DeletedOnUtc", "Name", "Id" },
                filter: "\"IsDeleted\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists",
                columns: new[] { "LibraryId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Albums",
                columns: new[] { "LibraryId", "IsDeleted", "DeletedOnUtc", "Title", "Id" },
                filter: "\"IsDeleted\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Artists_LibraryId_IsDeleted_DeletedOnUtc_Name_Id",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Albums_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists",
                columns: new[] { "LibraryId", "Name" },
                unique: true);
        }
    }
}
