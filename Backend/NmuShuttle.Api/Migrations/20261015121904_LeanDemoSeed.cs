using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class LeanDemoSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear the old convoy roster entirely before re-seeding the lean
            // one-shuttle-per-stop layout. Doing this as delete-all + insert avoids
            // transient violations of the (StopId, OrderPosition) unique index that
            // row-by-row UpdateData would otherwise cause.
            for (int id = 1; id <= 19; id++)
            {
                migrationBuilder.DeleteData(
                    table: "StopShuttleDefaults",
                    keyColumn: "StopShuttleDefaultId",
                    keyValue: id);
            }

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 19);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 1,
                column: "LicensePlate",
                value: "EC 101-001");

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 2,
                columns: new[] { "Capacity", "IsAccessibleVehicle", "LicensePlate" },
                values: new object[] { 2, true, "EC 102-002" });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 3,
                columns: new[] { "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { true, "EC 103-003", "Coastal Movers", 2 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 4,
                columns: new[] { "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { "EC 104-004", "Metro Link SA", 3 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 5,
                columns: new[] { "Capacity", "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 22, true, "EC 105-005", "Bay Transit Corp", 1 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 6,
                columns: new[] { "Capacity", "LicensePlate" },
                values: new object[] { 35, "EC 106-006" });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 7,
                column: "LicensePlate",
                value: "EC 107-007");

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 8,
                columns: new[] { "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { "EC 108-008", "Bay Transit Corp", 1 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 9,
                columns: new[] { "Capacity", "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 28, false, "EC 109-009", "Coastal Movers", 2 });

            migrationBuilder.InsertData(
                table: "StopShuttleDefaults",
                columns: new[] { "StopShuttleDefaultId", "CreatedAt", "OrderPosition", "ShuttleId", "StopId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 1 },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2, 2 },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3, 3 },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, 4 },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5, 5 },
                    { 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 6, 6 },
                    { 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 7, 7 },
                    { 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8, 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 1,
                column: "LicensePlate",
                value: "EC 142-837");

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 2,
                columns: new[] { "Capacity", "IsAccessibleVehicle", "LicensePlate" },
                values: new object[] { 35, false, "EC 256-491" });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 3,
                columns: new[] { "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { false, "EC 389-720", "Bay Transit Corp", 1 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 4,
                columns: new[] { "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { "EC 501-163", "Coastal Movers", 2 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 5,
                columns: new[] { "Capacity", "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 35, false, "EC 628-945", "Coastal Movers", 2 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 6,
                columns: new[] { "Capacity", "LicensePlate" },
                values: new object[] { 22, "EC 714-382" });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 7,
                column: "LicensePlate",
                value: "EC 837-256");

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 8,
                columns: new[] { "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { "EC 948-617", "Metro Link SA", 3 });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 9,
                columns: new[] { "Capacity", "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 22, true, "EC 103-475", "Metro Link SA", 3 });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[,]
                {
                    { 10, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 217-864", "Bay Transit Corp", 1 },
                    { 11, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 331-058", "Bay Transit Corp", 1 },
                    { 12, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 445-912", "Coastal Movers", 2 },
                    { 13, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 552-376", "Coastal Movers", 2 },
                    { 14, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 668-741", "Coastal Movers", 2 },
                    { 15, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 774-285", "Metro Link SA", 3 },
                    { 16, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 889-639", "Metro Link SA", 3 },
                    { 17, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 993-104", "Metro Link SA", 3 },
                    { 18, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 107-528", "Bay Transit Corp", 1 }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 19, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "EC 214-963", "Bay Transit Corp", 1 });

            // Restore the original convoy roster: clear the lean rows, then re-insert
            // the full 19-row layout (delete-all + insert to avoid unique-index clashes).
            for (int id = 1; id <= 8; id++)
            {
                migrationBuilder.DeleteData(
                    table: "StopShuttleDefaults",
                    keyColumn: "StopShuttleDefaultId",
                    keyValue: id);
            }

            migrationBuilder.InsertData(
                table: "StopShuttleDefaults",
                columns: new[] { "StopShuttleDefaultId", "CreatedAt", "OrderPosition", "ShuttleId", "StopId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 1 },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2, 1 },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 3, 3, 1 },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, 2 },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 5, 2 },
                    { 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 3, 6, 2 },
                    { 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 9, 3 },
                    { 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 7, 3 },
                    { 9, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 3, 8, 3 },
                    { 10, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 19, 4 },
                    { 11, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 10, 4 },
                    { 12, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 11, 5 },
                    { 13, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 12, 5 },
                    { 14, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 13, 6 },
                    { 15, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 14, 6 },
                    { 16, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 15, 7 },
                    { 17, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 16, 7 },
                    { 18, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 17, 8 },
                    { 19, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 18, 8 }
                });
        }
    }
}
