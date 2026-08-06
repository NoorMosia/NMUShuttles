using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminOtpFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiry",
                table: "Admins",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtpHash",
                table: "Admins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "AdminId",
                keyValue: 1,
                columns: new[] { "OtpExpiry", "OtpHash" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "AdminId",
                keyValue: 2,
                columns: new[] { "OtpExpiry", "OtpHash" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpExpiry",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "OtpHash",
                table: "Admins");
        }
    }
}
