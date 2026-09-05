using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticOrderWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                schema: "clinical",
                table: "diagnostic_orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                schema: "clinical",
                table: "diagnostic_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                schema: "clinical",
                table: "diagnostic_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PerformedAtUtc",
                schema: "clinical",
                table: "diagnostic_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PerformedByUserId",
                schema: "clinical",
                table: "diagnostic_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAtUtc",
                schema: "clinical",
                table: "diagnostic_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduledByUserId",
                schema: "clinical",
                table: "diagnostic_orders",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                schema: "clinical",
                table: "diagnostic_orders");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                schema: "clinical",
                table: "diagnostic_orders");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                schema: "clinical",
                table: "diagnostic_orders");

            migrationBuilder.DropColumn(
                name: "PerformedAtUtc",
                schema: "clinical",
                table: "diagnostic_orders");

            migrationBuilder.DropColumn(
                name: "PerformedByUserId",
                schema: "clinical",
                table: "diagnostic_orders");

            migrationBuilder.DropColumn(
                name: "ScheduledAtUtc",
                schema: "clinical",
                table: "diagnostic_orders");

            migrationBuilder.DropColumn(
                name: "ScheduledByUserId",
                schema: "clinical",
                table: "diagnostic_orders");
        }
    }
}
