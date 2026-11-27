using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddsDescriptionColumnToRolesAndPermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Id",
            table: "UserRoles");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "UserPermissions");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "RolePermissions");

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Roles",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Roles",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Roles",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Roles",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AddColumn<string>(
            name: "RoleDescription",
            table: "Roles",
            type: "TEXT",
            maxLength: 255,
            nullable: true)
            .Annotation("Relational:ColumnOrder", 2);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Permissions",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Permissions",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Permissions",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Permissions",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AddColumn<string>(
            name: "PermissionDescription",
            table: "Permissions",
            type: "TEXT",
            maxLength: 255,
            nullable: true)
            .Annotation("Relational:ColumnOrder", 2);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RoleDescription",
            table: "Roles");

        migrationBuilder.DropColumn(
            name: "PermissionDescription",
            table: "Permissions");

        migrationBuilder.AddColumn<Guid>(
            name: "Id",
            table: "UserRoles",
            type: "TEXT",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "Id",
            table: "UserPermissions",
            type: "TEXT",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Roles",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Roles",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Roles",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Roles",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AddColumn<Guid>(
            name: "Id",
            table: "RolePermissions",
            type: "TEXT",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Permissions",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Permissions",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Permissions",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Permissions",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);
    }
}
