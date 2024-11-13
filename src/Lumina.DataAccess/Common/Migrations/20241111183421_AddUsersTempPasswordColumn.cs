using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddUsersTempPasswordColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "VerificationTokenCreated",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<string>(
            name: "VerificationToken",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<string>(
            name: "TotpSecret",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AddColumn<string>(
            name: "TempPassword",
            table: "Users",
            type: "TEXT",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 3);

        migrationBuilder.AddColumn<DateTime>(
            name: "TempPasswordCreated",
            table: "Users",
            type: "TEXT",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 7);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TempPassword",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "TempPasswordCreated",
            table: "Users");

        migrationBuilder.AlterColumn<DateTime>(
            name: "VerificationTokenCreated",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<string>(
            name: "VerificationToken",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<string>(
            name: "TotpSecret",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);
    }
}
