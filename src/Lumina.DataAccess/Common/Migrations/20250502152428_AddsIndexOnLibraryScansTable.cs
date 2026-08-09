using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddsIndexOnLibraryScansTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_LibraryScans_LibraryId",
            table: "LibraryScans");

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScans_LibraryId_Status_CreatedOnUtc",
            table: "LibraryScans",
            columns: new[] { "LibraryId", "Status", "CreatedOnUtc" },
            descending: new[] { true, false, true });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_LibraryScans_LibraryId_Status_CreatedOnUtc",
            table: "LibraryScans");

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScans_LibraryId",
            table: "LibraryScans",
            column: "LibraryId");
    }
}
