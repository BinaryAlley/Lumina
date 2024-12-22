using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class ChangesUserRolesToUserRole : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_UserRoles",
            table: "UserRoles");

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserRoles",
            table: "UserRoles",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_UserRoles",
            table: "UserRoles");

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserRoles",
            table: "UserRoles",
            columns: new[] { "UserId", "RoleId" });
    }
}
