using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class SplitStopAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullAddress",
                table: "Stops");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Stops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Stops",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Stops",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Suburb",
                table: "Stops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 1,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6001", "12 Marine Drive", "Summerstrand" });

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 2,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6001", "45 Beach Road", "Humewood" });

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 3,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6045", "78 Cape Road", "Newton Park" });

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 4,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6070", "23 Heugh Road", "Walmer Heights" });

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 5,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6070", "91 Circular Drive", "Lorraine" });

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 6,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6210", "5 Seaview Crescent", "Bluewater Bay" });

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 7,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6045", "14 Langenhoven Drive", "Framesby" });

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 8,
                columns: new[] { "City", "PostalCode", "Street", "Suburb" },
                values: new object[] { "Gqeberha", "6045", "30 Ring Road", "Greenacres" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "Suburb",
                table: "Stops");

            migrationBuilder.AddColumn<string>(
                name: "FullAddress",
                table: "Stops",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 1,
                column: "FullAddress",
                value: "12 Marine Drive, Summerstrand, Gqeberha, 6001");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 2,
                column: "FullAddress",
                value: "45 Beach Road, Humewood, Gqeberha, 6001");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 3,
                column: "FullAddress",
                value: "78 Cape Road, Newton Park, Gqeberha, 6045");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 4,
                column: "FullAddress",
                value: "23 Heugh Road, Walmer Heights, Gqeberha, 6070");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 5,
                column: "FullAddress",
                value: "91 Circular Drive, Lorraine, Gqeberha, 6070");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 6,
                column: "FullAddress",
                value: "5 Seaview Crescent, Bluewater Bay, Gqeberha, 6210");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 7,
                column: "FullAddress",
                value: "14 Langenhoven Drive, Framesby, Gqeberha, 6045");

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopId",
                keyValue: 8,
                column: "FullAddress",
                value: "30 Ring Road, Greenacres, Gqeberha, 6045");
        }
    }
}
