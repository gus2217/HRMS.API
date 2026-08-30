using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.PatientRegistration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase1InsuranceAndClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShaNumber",
                schema: "patient",
                table: "patients",
                newName: "InsuranceNumber");

            migrationBuilder.AddColumn<string>(
                name: "ClinicType",
                schema: "patient",
                table: "patients",
                type: "character varying(48)",
                maxLength: 48,
                nullable: false,
                defaultValue: "GeneralOutpatient");

            migrationBuilder.AddColumn<string>(
                name: "InsuranceType",
                schema: "patient",
                table: "patients",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Private");

            // Existing rows: patients who had a SHA number are SHA-covered;
            // everyone else defaults to Private (self-pay).
            migrationBuilder.Sql(
                """UPDATE patient.patients SET "InsuranceType" = 'Sha' WHERE "InsuranceNumber" IS NOT NULL AND "InsuranceNumber" <> ''""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClinicType",
                schema: "patient",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "InsuranceType",
                schema: "patient",
                table: "patients");

            migrationBuilder.RenameColumn(
                name: "InsuranceNumber",
                schema: "patient",
                table: "patients",
                newName: "ShaNumber");
        }
    }
}
