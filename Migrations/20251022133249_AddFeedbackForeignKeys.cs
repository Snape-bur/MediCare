using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Patients_PatientId1",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_PatientId1",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "Feedbacks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId1",
                table: "Feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_PatientId1",
                table: "Feedbacks",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Patients_PatientId1",
                table: "Feedbacks",
                column: "PatientId1",
                principalTable: "Patients",
                principalColumn: "PatientId");
        }
    }
}
