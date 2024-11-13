using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddUsersTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                Password = table.Column<string>(type: "TEXT", nullable: false),
                TotpSecret = table.Column<string>(type: "TEXT", nullable: true),
                VerificationToken = table.Column<string>(type: "TEXT", nullable: true),
                VerificationTokenCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                Updated = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Users");
    }
}
