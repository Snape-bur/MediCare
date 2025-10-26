using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFeedbackTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Feedbacks");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Feedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_AppointmentId",
                table: "Feedbacks",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Appointments_AppointmentId",
                table: "Feedbacks",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Appointments_AppointmentId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_AppointmentId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Feedbacks");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
