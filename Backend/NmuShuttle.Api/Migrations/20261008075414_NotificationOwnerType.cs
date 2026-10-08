using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class NotificationOwnerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Students_StudentId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_StudentId_IsRead",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Notifications",
                newName: "OwnerId");

            // All pre-existing notifications belonged to students, so backfill
            // existing rows with 'Student' to satisfy CK_Notifications_OwnerType.
            migrationBuilder.AddColumn<string>(
                name: "OwnerType",
                table: "Notifications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Student");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Owner_IsRead",
                table: "Notifications",
                columns: new[] { "OwnerId", "OwnerType", "IsRead" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notifications_OwnerType",
                table: "Notifications",
                sql: "OwnerType IN ('Student','Marshal')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_Owner_IsRead",
                table: "Notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notifications_OwnerType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Notifications",
                newName: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_StudentId_IsRead",
                table: "Notifications",
                columns: new[] { "StudentId", "IsRead" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Students_StudentId",
                table: "Notifications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
