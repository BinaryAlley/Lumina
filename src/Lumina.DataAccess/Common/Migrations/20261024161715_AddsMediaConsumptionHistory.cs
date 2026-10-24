using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsMediaConsumptionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaConsumptionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaKind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StartedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastActivityOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConsumedDurationMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    LastPositionKind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LastPositionTimeOffsetMilliseconds = table.Column<long>(type: "INTEGER", nullable: true),
                    LastPositionLocation = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true),
                    LastProgressFraction = table.Column<double>(type: "REAL", nullable: false),
                    ConsecutivePagesRead = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaConsumptionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaConsumptionRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaResumePoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaKind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MediaItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastPositionKind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastPositionTimeOffsetMilliseconds = table.Column<long>(type: "INTEGER", nullable: true),
                    LastPositionLocation = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true),
                    LastProgressFraction = table.Column<double>(type: "REAL", nullable: false),
                    LastConsumedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaResumePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaResumePoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaConsumptionRecords_UserId_MediaKind_MediaItemId_LastActivityOnUtc",
                table: "MediaConsumptionRecords",
                columns: new[] { "UserId", "MediaKind", "MediaItemId", "LastActivityOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaConsumptionRecords_UserId_MediaKind_StartedOnUtc",
                table: "MediaConsumptionRecords",
                columns: new[] { "UserId", "MediaKind", "StartedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaResumePoints_UserId_MediaKind_MediaItemId",
                table: "MediaResumePoints",
                columns: new[] { "UserId", "MediaKind", "MediaItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaConsumptionRecords");

            migrationBuilder.DropTable(
                name: "MediaResumePoints");
        }
    }
}
