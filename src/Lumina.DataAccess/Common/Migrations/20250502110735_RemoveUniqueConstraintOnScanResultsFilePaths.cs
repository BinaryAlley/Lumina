using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class RemoveUniqueConstraintOnScanResultsFilePaths : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_LibraryScanResults_ContentHash_FileSize_Path",
            table: "LibraryScanResults");

        migrationBuilder.DropIndex(
            name: "IX_LibraryScanResults_Path",
            table: "LibraryScanResults");

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScanResults_ContentHash_FileSize_Path",
            table: "LibraryScanResults",
            columns: new[] { "ContentHash", "FileSize", "Path" });

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScanResults_Path",
            table: "LibraryScanResults",
            column: "Path");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_LibraryScanResults_ContentHash_FileSize_Path",
            table: "LibraryScanResults");

        migrationBuilder.DropIndex(
            name: "IX_LibraryScanResults_Path",
            table: "LibraryScanResults");

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScanResults_ContentHash_FileSize_Path",
            table: "LibraryScanResults",
            columns: new[] { "ContentHash", "FileSize", "Path" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScanResults_Path",
            table: "LibraryScanResults",
            column: "Path",
            unique: true);
    }
}
