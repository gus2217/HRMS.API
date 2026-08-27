using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clinical");

            migrationBuilder.CreateTable(
                name: "consultations",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicianUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TemperatureCelsius = table.Column<decimal>(type: "numeric", nullable: true),
                    BloodPressure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    PulseRate = table.Column<int>(type: "integer", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "integer", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clinical_notes",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinical_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinical_notes_consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "clinical",
                        principalTable: "consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "diagnoses",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IcdCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_diagnoses_consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "clinical",
                        principalTable: "consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_order_references",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusSnapshot = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_order_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lab_order_references_consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "clinical",
                        principalTable: "consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescription_orders",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prescription_orders_consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "clinical",
                        principalTable: "consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinical_notes_ConsultationId",
                schema: "clinical",
                table: "clinical_notes",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_diagnoses_ConsultationId",
                schema: "clinical",
                table: "diagnoses",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_references_ConsultationId",
                schema: "clinical",
                table: "lab_order_references",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_orders_ConsultationId",
                schema: "clinical",
                table: "prescription_orders",
                column: "ConsultationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinical_notes",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "diagnoses",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "lab_order_references",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "prescription_orders",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "consultations",
                schema: "clinical");
        }
    }
}
