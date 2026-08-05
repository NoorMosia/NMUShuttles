using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "Shuttles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    VendorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.VendorId);
                });

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 1,
                column: "VendorId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 2,
                column: "VendorId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 3,
                column: "VendorId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 4,
                column: "VendorId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 5,
                column: "VendorId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 6,
                column: "VendorId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 7,
                column: "VendorId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 8,
                column: "VendorId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 9,
                column: "VendorId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Shuttles",
                keyColumn: "ShuttleId",
                keyValue: 10,
                column: "VendorId",
                value: 1);

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "VendorId", "CompanyName", "ContactEmail", "ContactPhone", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "Bay Transit Corp", "info@baytransit.co.za", "041-555-0101", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Coastal Movers", "info@coastalmovers.co.za", "041-555-0202", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Metro Link SA", "info@metrolinksa.co.za", "041-555-0303", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shuttles_VendorId",
                table: "Shuttles",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "UQ_Vendors_CompanyName",
                table: "Vendors",
                column: "CompanyName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shuttles_Vendors_VendorId",
                table: "Shuttles",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "VendorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shuttles_Vendors_VendorId",
                table: "Shuttles");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Shuttles_VendorId",
                table: "Shuttles");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Shuttles");
        }
    }
}
