using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Laboratory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "laboratory");

            migrationBuilder.CreateTable(
                name: "lab_orders",
                schema: "laboratory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_lab_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lab_test_items",
                schema: "laboratory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TestName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResultValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResultUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ReferenceRange = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsAbnormal = table.Column<bool>(type: "boolean", nullable: true),
                    ResultedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_test_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lab_test_items_lab_orders_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "laboratory",
                        principalTable: "lab_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lab_test_items_LabOrderId",
                schema: "laboratory",
                table: "lab_test_items",
                column: "LabOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lab_test_items",
                schema: "laboratory");

            migrationBuilder.DropTable(
                name: "lab_orders",
                schema: "laboratory");
        }
    }
}
