using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.PatientRegistration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNationalRegistryLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalRegistryNumber",
                schema: "patient",
                table: "patients",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalRegistryNumber",
                schema: "patient",
                table: "patients");
        }
    }
}
