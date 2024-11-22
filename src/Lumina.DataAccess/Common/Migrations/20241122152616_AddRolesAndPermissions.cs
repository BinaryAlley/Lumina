using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddRolesAndPermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Updated",
            table: "Users");

        migrationBuilder.RenameColumn(
            name: "VerificationTokenCreated",
            table: "Users",
            newName: "UpdatedOnUtc");

        migrationBuilder.RenameColumn(
            name: "VerificationToken",
            table: "Users",
            newName: "UpdatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            table: "Users",
            newName: "CreatedOnUtc");

        migrationBuilder.AlterColumn<DateTime>(
            name: "TempPasswordCreated",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Users",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 6);

        migrationBuilder.AddColumn<Guid>(
            name: "CreatedBy",
            table: "Users",
            type: "TEXT",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
            .Annotation("Relational:ColumnOrder", 7);

        migrationBuilder.CreateTable(
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                PermissionName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                RoleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserPermissions",
            columns: table => new
            {
                CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                PermissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                Id = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                table.ForeignKey(
                    name: "FK_UserPermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserPermissions_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                PermissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                Id = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                table.ForeignKey(
                    name: "FK_RolePermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            columns: table => new
            {
                CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                Id = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_UserRoles_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserRoles_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Username",
            table: "Users",
            column: "Username",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_PermissionId",
            table: "RolePermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_RoleId_PermissionId",
            table: "RolePermissions",
            columns: new[] { "RoleId", "PermissionId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserPermissions_PermissionId",
            table: "UserPermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_UserPermissions_UserId_PermissionId",
            table: "UserPermissions",
            columns: new[] { "UserId", "PermissionId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_RoleId",
            table: "UserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_UserId_RoleId",
            table: "UserRoles",
            columns: new[] { "UserId", "RoleId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RolePermissions");

        migrationBuilder.DropTable(
            name: "UserPermissions");

        migrationBuilder.DropTable(
            name: "UserRoles");

        migrationBuilder.DropTable(
            name: "Permissions");

        migrationBuilder.DropTable(
            name: "Roles");

        migrationBuilder.DropIndex(
            name: "IX_Users_Username",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Users");

        migrationBuilder.RenameColumn(
            name: "UpdatedOnUtc",
            table: "Users",
            newName: "VerificationTokenCreated");

        migrationBuilder.RenameColumn(
            name: "UpdatedBy",
            table: "Users",
            newName: "VerificationToken");

        migrationBuilder.RenameColumn(
            name: "CreatedOnUtc",
            table: "Users",
            newName: "Created");

        migrationBuilder.AlterColumn<DateTime>(
            name: "TempPasswordCreated",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<DateTime>(
            name: "VerificationTokenCreated",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<string>(
            name: "VerificationToken",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "Created",
            table: "Users",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AddColumn<DateTime>(
            name: "Updated",
            table: "Users",
            type: "TEXT",
            nullable: true);
    }
}
