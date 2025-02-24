using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddsMediaLibraryScanResultsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LibraryScanResults",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Path = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                ContentHash = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                LibraryScanId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LibraryScanResults", x => new { x.LibraryScanId, x.Path });
                table.ForeignKey(
                    name: "FK_LibraryScanResults_LibraryScans_LibraryScanId",
                    column: x => x.LibraryScanId,
                    principalTable: "LibraryScans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LibraryScanResults");
    }
}
