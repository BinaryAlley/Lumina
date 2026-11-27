using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class ConvertsRoleFromEnumToString : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RoleDescription",
            table: "Roles");

        migrationBuilder.DropColumn(
            name: "PermissionDescription",
            table: "Permissions");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RoleDescription",
            table: "Roles",
            type: "TEXT",
            maxLength: 255,
            nullable: true)
            .Annotation("Relational:ColumnOrder", 2);

        migrationBuilder.AddColumn<string>(
            name: "PermissionDescription",
            table: "Permissions",
            type: "TEXT",
            maxLength: 255,
            nullable: true)
            .Annotation("Relational:ColumnOrder", 2);
    }
}
