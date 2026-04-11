using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPhoneVerified",
                table: "Users",
                newName: "CanResetPassword");

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetAllowedUntil",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetAllowedUntil",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "CanResetPassword",
                table: "Users",
                newName: "IsPhoneVerified");
        }
    }
}
