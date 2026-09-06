using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.PatientRegistration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKenyaEmrPatientDemographics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternativePhone",
                schema: "patient",
                table: "patients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighestEducation",
                schema: "patient",
                table: "patients",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Landmark",
                schema: "patient",
                table: "patients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                schema: "patient",
                table: "patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                schema: "patient",
                table: "patients",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                schema: "patient",
                table: "patients",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativePhone",
                schema: "patient",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "HighestEducation",
                schema: "patient",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "Landmark",
                schema: "patient",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                schema: "patient",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "Occupation",
                schema: "patient",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "Village",
                schema: "patient",
                table: "patients");
        }
    }
}
