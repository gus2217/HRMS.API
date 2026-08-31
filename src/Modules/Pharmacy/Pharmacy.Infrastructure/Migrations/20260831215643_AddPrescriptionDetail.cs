using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                schema: "pharmacy",
                table: "prescription_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                schema: "pharmacy",
                table: "prescription_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Route",
                schema: "pharmacy",
                table: "prescription_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationDays",
                schema: "pharmacy",
                table: "prescription_items");

            migrationBuilder.DropColumn(
                name: "Frequency",
                schema: "pharmacy",
                table: "prescription_items");

            migrationBuilder.DropColumn(
                name: "Route",
                schema: "pharmacy",
                table: "prescription_items");
        }
    }
}
