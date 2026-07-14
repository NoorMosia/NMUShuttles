using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStops7And8SchedulesAndNewShuttles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ScheduleTimes",
                columns: new[] { "ScheduleTimeId", "CampusId", "DepartureTime", "Direction", "ScheduleType", "StopId" },
                values: new object[,]
                {
                    { 19, 1, new TimeOnly(7, 45, 0), "ToCampus", "Weekday", 7 },
                    { 20, 2, new TimeOnly(8, 0, 0), "ToCampus", "Weekday", 8 },
                    { 21, 1, new TimeOnly(16, 30, 0), "FromCampus", "Weekday", 7 },
                    { 22, 2, new TimeOnly(17, 0, 0), "FromCampus", "Weekday", 8 }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName" },
                values: new object[] { 9, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 103-475", "Metro Link SA" });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "LicensePlate", "VendorCompanyName" },
                values: new object[] { 10, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "EC 217-864", "Bay Transit Corp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "ScheduleTimeId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 10);
        }
    }
}
