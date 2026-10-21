using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsVuMeterSettingsToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Volume",
                table: "UserSettings",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL")
                .Annotation("Relational:ColumnOrder", 28)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<bool>(
                name: "IsMuted",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 29)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<double>(
                name: "VuMeterHighThreshold",
                table: "UserSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 0.8)
                .Annotation("Relational:ColumnOrder", 23);

            migrationBuilder.AddColumn<double>(
                name: "VuMeterMediumThreshold",
                table: "UserSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 0.6)
                .Annotation("Relational:ColumnOrder", 22);

            migrationBuilder.AddColumn<int>(
                name: "VuMeterOrientation",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AddColumn<int>(
                name: "VuMeterPeakHoldDurationInMilliseconds",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 650)
                .Annotation("Relational:ColumnOrder", 25);

            migrationBuilder.AddColumn<double>(
                name: "VuMeterPeakReleasePerSecond",
                table: "UserSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 0.65)
                .Annotation("Relational:ColumnOrder", 27);

            migrationBuilder.AddColumn<double>(
                name: "VuMeterReleasePerSecond",
                table: "UserSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 3.0)
                .Annotation("Relational:ColumnOrder", 26);

            migrationBuilder.AddColumn<int>(
                name: "VuMeterSegmentCount",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 12)
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<int>(
                name: "VuMeterSegmentGap",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1)
                .Annotation("Relational:ColumnOrder", 21);

            migrationBuilder.AddColumn<bool>(
                name: "VuMeterShowPeak",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:ColumnOrder", 24);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VuMeterHighThreshold",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterMediumThreshold",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterOrientation",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterPeakHoldDurationInMilliseconds",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterPeakReleasePerSecond",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterReleasePerSecond",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterSegmentCount",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterSegmentGap",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "VuMeterShowPeak",
                table: "UserSettings");

            migrationBuilder.AlterColumn<double>(
                name: "Volume",
                table: "UserSettings",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL")
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 28);

            migrationBuilder.AlterColumn<bool>(
                name: "IsMuted",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 29);
        }
    }
}
