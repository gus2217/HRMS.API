using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceLineStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceReferenceId",
                schema: "billing",
                table: "invoice_lines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                schema: "billing",
                table: "invoice_lines",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "billing",
                table: "invoice_lines",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceReferenceId",
                schema: "billing",
                table: "invoice_lines");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "billing",
                table: "invoice_lines");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "billing",
                table: "invoice_lines");
        }
    }
}
