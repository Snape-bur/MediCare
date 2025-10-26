using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Migrations
{
    /// <inheritdoc />
    public partial class LinkDoctorWithAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilitySchedule",
                table: "Doctors");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "Availabilities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_DoctorId1",
                table: "Availabilities",
                column: "DoctorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_Doctors_DoctorId1",
                table: "Availabilities",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_Doctors_DoctorId1",
                table: "Availabilities");

            migrationBuilder.DropIndex(
                name: "IX_Availabilities_DoctorId1",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "Availabilities");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilitySchedule",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
