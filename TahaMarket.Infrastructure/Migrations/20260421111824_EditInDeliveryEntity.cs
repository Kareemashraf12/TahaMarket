using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditInDeliveryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Deliveries");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Deliveries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Deliveries");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Deliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Deliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
