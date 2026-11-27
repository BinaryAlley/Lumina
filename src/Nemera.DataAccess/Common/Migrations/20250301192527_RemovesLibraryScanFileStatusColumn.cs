using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class RemovesLibraryScanFileStatusColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Status",
            table: "LibraryScanResults");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "LibraryScanResults",
            type: "TEXT",
            maxLength: 10,
            nullable: false,
            defaultValue: "")
            .Annotation("Relational:ColumnOrder", 5);
    }
}
