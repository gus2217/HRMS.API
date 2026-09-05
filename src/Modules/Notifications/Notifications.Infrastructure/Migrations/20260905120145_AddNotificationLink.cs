using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jacana.Notifications.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                schema: "notifications",
                table: "user_notifications",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                schema: "notifications",
                table: "user_notifications");
        }
    }
}
