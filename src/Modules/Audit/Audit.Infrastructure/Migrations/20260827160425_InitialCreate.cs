using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    BeforeValuesJson = table.Column<string>(type: "text", nullable: true),
                    AfterValuesJson = table.Column<string>(type: "text", nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "audit");
        }
    }
}
