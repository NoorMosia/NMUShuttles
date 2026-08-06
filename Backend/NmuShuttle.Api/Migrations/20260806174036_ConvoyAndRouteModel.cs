using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConvoyAndRouteModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Shuttles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresScanning",
                table: "ScheduleTimes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "ScheduleTimeCampuses",
                columns: table => new
                {
                    ScheduleTimeCampusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleTimeId = table.Column<int>(type: "int", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    StopOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleTimeCampuses", x => x.ScheduleTimeCampusId);
                    table.ForeignKey(
                        name: "FK_ScheduleTimeCampuses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleTimeCampuses_ScheduleTimes_ScheduleTimeId",
                        column: x => x.ScheduleTimeId,
                        principalTable: "ScheduleTimes",
                        principalColumn: "ScheduleTimeId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Data migration: populate ScheduleTimeCampuses from existing CampusId before dropping
            migrationBuilder.Sql(@"
                INSERT INTO ScheduleTimeCampuses (ScheduleTimeId, CampusId, StopOrder)
                SELECT ScheduleTimeId, CampusId, 1
                FROM ScheduleTimes
                WHERE CampusId IS NOT NULL
            ");

            // Now safe to drop the old column
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleTimes_Campuses_CampusId",
                table: "ScheduleTimes");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleTimes_CampusId",
                table: "ScheduleTimes");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "ScheduleTimes");

            migrationBuilder.CreateTable(
                name: "StopShuttleDefaults",
                columns: table => new
                {
                    StopShuttleDefaultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StopId = table.Column<int>(type: "int", nullable: false),
                    ShuttleId = table.Column<int>(type: "int", nullable: false),
                    OrderPosition = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopShuttleDefaults", x => x.StopShuttleDefaultId);
                    table.ForeignKey(
                        name: "FK_StopShuttleDefaults_Shuttles_ShuttleId",
                        column: x => x.ShuttleId,
                        principalTable: "Shuttles",
                        principalColumn: "ShuttleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StopShuttleDefaults_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "StopId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripNotes",
                columns: table => new
                {
                    TripNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripInstanceId = table.Column<int>(type: "int", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsResolutionAction = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripNotes", x => x.TripNoteId);
                    table.CheckConstraint("CK_TripNotes_NoteType", "NoteType IN ('Incident','Replacement','General','NoShow')");
                    table.ForeignKey(
                        name: "FK_TripNotes_Admins_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Admins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripNotes_TripInstances_TripInstanceId",
                        column: x => x.TripInstanceId,
                        principalTable: "TripInstances",
                        principalColumn: "TripInstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 1,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 2,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 3,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 4,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 5,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 6,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 7,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 8,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 9,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 10,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 11,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 12,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 13,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 14,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 15,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 16,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 17,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 18,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 19,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 20,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 21,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 22,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 23,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 24,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 25,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 26,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 27,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 28,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 29,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 30,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 31,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 32,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 33,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 34,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 35,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 36,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 37,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 38,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 39,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 40,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 41,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 42,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 43,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 44,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 45,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 46,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 47,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 48,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 49,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 50,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 51,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 52,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 53,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 54,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 55,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 56,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 57,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 58,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 59,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 60,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 61,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 62,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 63,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 64,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 65,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 66,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 67,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 68,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 69,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 70,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 71,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 72,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 73,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 74,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 75,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 76,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 77,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 78,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 79,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 80,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 81,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 82,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 83,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 84,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 85,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 86,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 87,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 88,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 89,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 90,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 91,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 92,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 93,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 94,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 95,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 96,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 97,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 98,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 99,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 100,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 101,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 102,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 103,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 104,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 105,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 106,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 107,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 108,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 109,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 110,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 111,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 112,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 113,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 114,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 115,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 116,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 117,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 118,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 119,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 120,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 121,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 122,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 123,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 124,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 125,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 126,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 127,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 128,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 129,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 130,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 131,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 132,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 133,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 134,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 135,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 136,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 137,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 138,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 139,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 140,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 141,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 142,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 143,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 144,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 145,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 146,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 147,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 148,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 149,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 150,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 151,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 152,
                column: "RequiresScanning",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 1,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 2,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 3,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 4,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 5,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 6,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 7,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 8,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 9,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 10,
                column: "IsActive",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shuttles_IsActive",
                table: "Shuttles",
                column: "IsActive",
                filter: "IsActive = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimeCampuses_CampusId",
                table: "ScheduleTimeCampuses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimeCampuses_ScheduleTimeId",
                table: "ScheduleTimeCampuses",
                column: "ScheduleTimeId");

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleTimeCampuses_ScheduleTime_Campus",
                table: "ScheduleTimeCampuses",
                columns: new[] { "ScheduleTimeId", "CampusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleTimeCampuses_ScheduleTime_StopOrder",
                table: "ScheduleTimeCampuses",
                columns: new[] { "ScheduleTimeId", "StopOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StopShuttleDefaults_ShuttleId",
                table: "StopShuttleDefaults",
                column: "ShuttleId");

            migrationBuilder.CreateIndex(
                name: "IX_StopShuttleDefaults_StopId",
                table: "StopShuttleDefaults",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "UQ_StopShuttleDefaults_Stop_OrderPosition",
                table: "StopShuttleDefaults",
                columns: new[] { "StopId", "OrderPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_StopShuttleDefaults_Stop_Shuttle",
                table: "StopShuttleDefaults",
                columns: new[] { "StopId", "ShuttleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripNotes_CreatedByAdminId",
                table: "TripNotes",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TripNotes_TripInstanceId",
                table: "TripNotes",
                column: "TripInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TripNotes_TripInstanceId_NoteType",
                table: "TripNotes",
                columns: new[] { "TripInstanceId", "NoteType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleTimeCampuses");

            migrationBuilder.DropTable(
                name: "StopShuttleDefaults");

            migrationBuilder.DropTable(
                name: "TripNotes");

            migrationBuilder.DropIndex(
                name: "IX_Shuttles_IsActive",
                table: "Shuttles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Shuttles");

            migrationBuilder.DropColumn(
                name: "RequiresScanning",
                table: "ScheduleTimes");

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "ScheduleTimes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 1,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 2,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 3,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 4,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 5,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 6,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 7,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 8,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 9,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 10,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 11,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 12,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 13,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 14,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 15,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 16,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 17,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 18,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 19,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 20,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 21,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 22,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 23,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 24,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 25,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 26,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 27,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 28,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 29,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 30,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 31,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 32,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 33,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 34,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 35,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 36,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 37,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 38,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 39,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 40,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 41,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 42,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 43,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 44,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 45,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 46,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 47,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 48,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 49,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 50,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 51,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 52,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 53,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 54,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 55,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 56,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 57,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 58,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 59,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 60,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 61,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 62,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 63,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 64,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 65,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 66,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 67,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 68,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 69,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 70,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 71,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 72,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 73,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 74,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 75,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 76,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 77,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 78,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 79,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 80,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 81,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 82,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 83,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 84,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 85,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 86,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 87,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 88,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 89,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 90,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 91,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 92,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 93,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 94,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 95,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 96,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 97,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 98,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 99,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 100,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 101,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 102,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 103,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 104,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 105,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 106,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 107,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 108,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 109,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 110,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 111,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 112,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 113,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 114,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 115,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 116,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 117,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 118,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 119,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 120,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 121,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 122,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 123,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 124,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 125,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 126,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 127,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 128,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 129,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 130,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 131,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 132,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 133,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 134,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 135,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 136,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 137,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 138,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 139,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 140,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 141,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 142,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 143,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 144,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 145,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 146,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 147,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 148,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 149,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 150,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 151,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 152,
                column: "CampusId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimes_CampusId",
                table: "ScheduleTimes",
                column: "CampusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleTimes_Campuses_CampusId",
                table: "ScheduleTimes",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
