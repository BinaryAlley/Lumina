using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsScheduledJobsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledJobExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScheduledJobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskType = table.Column<string>(type: "TEXT", nullable: false),
                    IsCycleRun = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJobExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    TaskType = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduleType = table.Column<string>(type: "TEXT", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    Hour = table.Column<int>(type: "INTEGER", nullable: true),
                    Minute = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastStartedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCompletedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobExecutions_ScheduledJobId",
                table: "ScheduledJobExecutions",
                column: "ScheduledJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobExecutions_StartedOnUtc_ScheduledJobId",
                table: "ScheduledJobExecutions",
                columns: new[] { "StartedOnUtc", "ScheduledJobId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_Status",
                table: "ScheduledJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledJobExecutions");

            migrationBuilder.DropTable(
                name: "ScheduledJobs");
        }
    }
}
