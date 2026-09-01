using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagsAttachmentsDiagnosticOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "diagnostic_orders",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodySite = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClinicalIndication = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OrderedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Report = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_diagnostic_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patient_attachments",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_patient_attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patient_flags",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeactivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeactivatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_flags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_diagnostic_orders_ConsultationId",
                schema: "clinical",
                table: "diagnostic_orders",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_diagnostic_orders_PatientId_Status",
                schema: "clinical",
                table: "diagnostic_orders",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_attachments_PatientId_UploadedAtUtc",
                schema: "clinical",
                table: "patient_attachments",
                columns: new[] { "PatientId", "UploadedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_flags_PatientId_IsActive",
                schema: "clinical",
                table: "patient_flags",
                columns: new[] { "PatientId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diagnostic_orders",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "patient_attachments",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "patient_flags",
                schema: "clinical");
        }
    }
}
