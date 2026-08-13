using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class DropMarshalAssignmentConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Marshals_AssignedXOR",
                table: "Marshals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Marshals_AssignedXOR",
                table: "Marshals",
                sql: "1=1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Marshals_AssignedXOR",
                table: "Marshals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Marshals_AssignedXOR",
                table: "Marshals",
                sql: "AssignedCampusId IS NOT NULL OR AssignedStopId IS NOT NULL");
        }
    }
}
