using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class ChangesLibraryScanResultContentHashColumnToUlong : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<ulong>(
            name: "ContentHash",
            table: "LibraryScanResults",
            type: "INTEGER",
            maxLength: 24,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 24);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ContentHash",
            table: "LibraryScanResults",
            type: "TEXT",
            maxLength: 24,
            nullable: false,
            oldClrType: typeof(ulong),
            oldType: "INTEGER",
            oldMaxLength: 24);
    }
}
