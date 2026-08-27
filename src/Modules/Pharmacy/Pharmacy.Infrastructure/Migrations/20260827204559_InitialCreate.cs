using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pharmacy");

            migrationBuilder.CreateTable(
                name: "dispense_records",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityDispensed = table.Column<int>(type: "integer", nullable: false),
                    DispensedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispensedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispense_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prescriptions",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescribedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescribedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prescription_items",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DosageInstructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QuantityPrescribed = table.Column<int>(type: "integer", nullable: false),
                    QuantityDispensed = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prescription_items_prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "pharmacy",
                        principalTable: "prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_PrescriptionId",
                schema: "pharmacy",
                table: "prescription_items",
                column: "PrescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispense_records",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "prescription_items",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "prescriptions",
                schema: "pharmacy");
        }
    }
}
