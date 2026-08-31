using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDrugCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "inventory",
                table: "drugs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                schema: "inventory",
                table: "drugs");
        }
    }
}
