using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCare.Migrations
{
    /// <inheritdoc />
    public partial class ReconnectDoctorSpecialtyFK_Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Do NOT reinsert specialties to avoid PK duplicates
            // ✅ Only re-add the Doctor → Specialty foreign key

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Specialties_SpecialtyId",
                table: "Doctors",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Just remove the FK if rolling back
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Specialties_SpecialtyId",
                table: "Doctors");
        }
    }
}
