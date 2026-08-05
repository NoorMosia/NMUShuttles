using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class RelaxMarshalAssignmentConstraint : Migration
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
                sql: "(AssignedCampusId IS NULL AND AssignedStopId IS NULL) OR (AssignedCampusId IS NOT NULL AND AssignedStopId IS NULL) OR (AssignedCampusId IS NULL AND AssignedStopId IS NOT NULL)");
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
                sql: "(AssignedCampusId IS NOT NULL AND AssignedStopId IS NULL) OR (AssignedCampusId IS NULL AND AssignedStopId IS NOT NULL)");
        }
    }
}
