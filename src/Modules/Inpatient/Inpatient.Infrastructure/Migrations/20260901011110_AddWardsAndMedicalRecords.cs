using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Inpatient.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWardsAndMedicalRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdmittingDiagnosis",
                schema: "inpatient",
                table: "admissions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AttendingClinicianUserId",
                schema: "inpatient",
                table: "admissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WardId",
                schema: "inpatient",
                table: "admissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ward_medical_records",
                schema: "inpatient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TemperatureCelsius = table.Column<decimal>(type: "numeric", nullable: true),
                    SystolicBp = table.Column<int>(type: "integer", nullable: true),
                    DiastolicBp = table.Column<int>(type: "integer", nullable: true),
                    PulseRate = table.Column<int>(type: "integer", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "integer", nullable: true),
                    OxygenSaturation = table.Column<int>(type: "integer", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    Subjective = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Objective = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Assessment = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Plan = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ward_medical_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ward_medical_records_admissions_AdmissionId",
                        column: x => x.AdmissionId,
                        principalSchema: "inpatient",
                        principalTable: "admissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wards",
                schema: "inpatient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalBeds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_wards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ward_record_attachments",
                schema: "inpatient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WardMedicalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ward_record_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ward_record_attachments_ward_medical_records_WardMedicalRec~",
                        column: x => x.WardMedicalRecordId,
                        principalSchema: "inpatient",
                        principalTable: "ward_medical_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ward_medical_records_AdmissionId",
                schema: "inpatient",
                table: "ward_medical_records",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ward_record_attachments_WardMedicalRecordId",
                schema: "inpatient",
                table: "ward_record_attachments",
                column: "WardMedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_wards_IsActive",
                schema: "inpatient",
                table: "wards",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ward_record_attachments",
                schema: "inpatient");

            migrationBuilder.DropTable(
                name: "wards",
                schema: "inpatient");

            migrationBuilder.DropTable(
                name: "ward_medical_records",
                schema: "inpatient");

            migrationBuilder.DropColumn(
                name: "AdmittingDiagnosis",
                schema: "inpatient",
                table: "admissions");

            migrationBuilder.DropColumn(
                name: "AttendingClinicianUserId",
                schema: "inpatient",
                table: "admissions");

            migrationBuilder.DropColumn(
                name: "WardId",
                schema: "inpatient",
                table: "admissions");
        }
    }
}
