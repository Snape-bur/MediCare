using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MediCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSpecialties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Specialties",
                columns: new[] { "SpecialtyId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Primary care and general health services", "General Medicine" },
                    { 2, "Child health and development", "Pediatrics" },
                    { 3, "Heart and cardiovascular care", "Cardiology" },
                    { 4, "Skin conditions and treatments", "Dermatology" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: 4);
        }
    }
}
